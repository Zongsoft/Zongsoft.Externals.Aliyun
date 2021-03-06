﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2016 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Externals.Aliyun.
 *
 * Zongsoft.Externals.Aliyun is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Externals.Aliyun is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Externals.Aliyun; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Zongsoft.Externals.Aliyun.Messaging
{
	public class Message : Zongsoft.Messaging.MessageBase
	{
		#region 常量定义
		private static readonly Regex _delay_regex = new Regex(@"\<(?'tag'(ReceiptHandle|NextVisibleTime))\>\s*(?<value>[^<>\s]+)\s*\<\/\k'tag'\>", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
		#endregion

		#region 成员字段
		private MessageQueue _queue;
		#endregion

		#region 构造函数
		public Message(MessageQueue queue, string id, byte[] data, byte[] checksum = null, DateTime? expires = null, DateTime? enqueuedTime = null, DateTime? dequeuedTime = null, int dequeuedCount = 0)
			: base(id, data, checksum, expires, enqueuedTime, dequeuedTime, dequeuedCount)
		{
			_queue = queue ?? throw new ArgumentNullException(nameof(queue));
		}
		#endregion

		#region 公共方法
		public async Task<bool> DeleteAsync()
		{
			if(string.IsNullOrEmpty(this.AcknowledgementId))
				return false;

			var http = _queue.Http;
			var request = new HttpRequestMessage(HttpMethod.Delete, _queue.GetRequestUrl("messages") + "?ReceiptHandle=" + Uri.EscapeDataString(this.AcknowledgementId));
			request.Headers.Add("x-mns-version", "2015-06-06");
			var response = await http.SendAsync(request);
			return response.IsSuccessStatusCode;
		}

		public override async Task<DateTime> DelayAsync(TimeSpan duration)
		{
			if(string.IsNullOrEmpty(this.AcknowledgementId))
				return this.Expires;

			var http = _queue.Http;
			var request = new HttpRequestMessage(HttpMethod.Put, _queue.GetRequestUrl("messages") + "?ReceiptHandle=" + Uri.EscapeDataString(this.AcknowledgementId) + "&VisibilityTimeout=" + duration.TotalSeconds.ToString());
			request.Headers.Add("x-mns-version", "2015-06-06");
			var response = await http.SendAsync(request);

			if(!response.IsSuccessStatusCode)
			{
				var exception = AliyunException.Parse(await response.Content.ReadAsStringAsync());

				if(exception != null)
					throw exception;

				response.EnsureSuccessStatusCode();
			}

			var matches = _delay_regex.Matches(await response.Content.ReadAsStringAsync());

			foreach(Match match in matches)
			{
				switch(match.Groups["tag"].Value)
				{
					case "ReceiptHandle":
						this.AcknowledgementId = match.Groups["value"].Value;
						break;
					case "NextVisibleTime":
						this.Expires = Utility.GetDateTimeFromEpoch(match.Groups["value"].Value);
						break;
				}
			}

			return this.Expires;
		}

		public override async Task<object> AcknowledgeAsync(object parameter = null)
		{
			if(parameter == null)
				return await this.DeleteAsync();

			TimeSpan? duration = Utility.GetDuration(parameter, this.Expires);

			if(duration.HasValue)
				return await this.DelayAsync(duration.Value);

			throw new ArgumentException(string.Format("The '{0}' value of parameter is not supported.", parameter));
		}
		#endregion

		#region 内部方法
		internal byte InnerPriority
		{
			set
			{
				this.Priority = value;
			}
		}

		internal string InnerAcknowledgementId
		{
			set
			{
				this.AcknowledgementId = value;
			}
		}
		#endregion
	}
}

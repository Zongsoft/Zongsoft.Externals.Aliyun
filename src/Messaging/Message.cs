/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

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
		public Message(MessageQueue queue, string id, byte[] data, byte[] checksum = null, System.DateTime? expires = null, System.DateTime? enqueuedTime = null, System.DateTime? dequeuedTime = null, int dequeuedCount = 0)
			: base(id, data, checksum, expires, enqueuedTime, dequeuedTime, dequeuedCount)
		{
			if(queue == null)
				throw new ArgumentNullException("queue");

			_queue = queue;
		}
		#endregion

		#region 公共方法
		public async Task<bool> DeleteAsync()
		{
			if(string.IsNullOrEmpty(this.AcknowledgementId))
				return false;

			var client = _queue.CreateHttpClient();
			var response = await client.DeleteAsync(_queue.GetRequestUrl("messages") + "?ReceiptHandle=" + Uri.EscapeDataString(this.AcknowledgementId));
			return response.IsSuccessStatusCode;
		}

		public override async Task<DateTime> DelayAsync(TimeSpan duration)
		{
			if(string.IsNullOrEmpty(this.AcknowledgementId))
				return this.Expires;

			var client = _queue.CreateHttpClient();
			var response = await client.PutAsync(_queue.GetRequestUrl("messages") + "?ReceiptHandle=" + Uri.EscapeDataString(this.AcknowledgementId) + "&VisibilityTimeout=" + duration.TotalSeconds.ToString(), null);

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
						this.Expires = Utility.GetExpiresTimeFromMilliseconds(match.Groups["value"].Value);
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

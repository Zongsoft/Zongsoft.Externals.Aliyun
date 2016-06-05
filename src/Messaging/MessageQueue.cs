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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Zongsoft.Externals.Aliyun.Messaging
{
	public class MessageQueue : Zongsoft.Messaging.MessageQueueBase
	{
		#region 常量定义
		private static readonly Regex _count_regex = new Regex(@"\<(?'tag'(ActiveMessages|InactiveMessages|DelayMessages))\>\s*(?<value>[^<>\s]+)\s*\<\/\k'tag'\>", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
		#endregion

		#region 成员字段
		private MessageQueueProvider _provider;
		#endregion

		#region 构造函数
		internal MessageQueue(MessageQueueProvider provider, string name) : base(name)
		{
			if(provider == null)
				throw new ArgumentNullException("provider");

			_provider = provider;
		}
		#endregion

		#region 重写方法
		public override async Task<long> GetCountAsync()
		{
			var client = this.CreateHttpClient();
			var response = await client.GetAsync(this.GetRequestUrl());

			if(!response.IsSuccessStatusCode)
				return 0;

			var content = await response.Content.ReadAsStringAsync();

			if(string.IsNullOrWhiteSpace(content))
				return 0;

			var matches = _count_regex.Matches(content);

			if(matches == null || matches.Count < 1)
				return 0;

			long total = 0;

			foreach(Match match in matches)
			{
				if(match.Success)
					total += Zongsoft.Common.Convert.ConvertValue<long>(match.Groups["value"].Value, 0);
			}

			return total;
		}

		public override async Task<int> EnqueueManyAsync<T>(IEnumerable<T> items, Zongsoft.Messaging.MessageEnqueueSettings settings = null)
		{
			if(items == null)
				return 0;

			var count = 0;
			var client = this.CreateHttpClient();

			foreach(var item in items)
			{
				var content = this.SerializeContent(item);

				if(content == null)
					continue;

				byte priority = 8;
				TimeSpan? duration = null;

				if(settings != null)
				{
					priority = settings.Priority;
					duration = settings.DelayTimeout;
				}

				if(duration.HasValue && duration.Value.TotalDays > 7)
					throw new ArgumentOutOfRangeException("settings", "The duration must less than 7 days.");

				var text = @"<Message xmlns=""http://mqs.aliyuncs.com/doc/v1/""><MessageBody>" +
					System.Convert.ToBase64String(content) +
					"</MessageBody><DelaySeconds>" +
					(duration.HasValue ? duration.Value.TotalSeconds.ToString() : "0") +
					"</DelaySeconds><Priority>" + priority.ToString() + "</Priority></Message>";

				var request = new HttpRequestMessage(HttpMethod.Post, this.GetRequestUrl("messages"));
				request.Content = new StringContent(text, Encoding.UTF8, "text/xml");
				request.Headers.Add("x-mqs-version", "2015-06-06");

				var response = await client.SendAsync(request);

				if(!response.IsSuccessStatusCode)
				{
					return count;
				}

				count++;
			}

			return count;
		}

		public override Zongsoft.Messaging.MessageBase Dequeue(Zongsoft.Messaging.MessageDequeueSettings settings = null)
		{
			if(settings == null)
				return Utility.ExecuteTask(() => this.DequeueOrPeekAsync(0));
			else
				return Utility.ExecuteTask(() => this.DequeueOrPeekAsync((int)settings.PollingTimeout.TotalSeconds));
		}

		public override IEnumerable<Zongsoft.Messaging.MessageBase> Dequeue(int count, Zongsoft.Messaging.MessageDequeueSettings settings = null)
		{
			if(count < 1)
				throw new ArgumentOutOfRangeException("count");

			var messages = new List<Zongsoft.Messaging.MessageBase>(count);

			for(int i = 0; i < count; i++)
			{
				var message = this.Dequeue(settings);

				//如果返回的结果为空则表示队列已空
				if(message == null)
					return messages;

				messages.Add(message);
			}

			return messages;
		}

		public override Task<Zongsoft.Messaging.MessageBase> DequeueAsync(Zongsoft.Messaging.MessageDequeueSettings settings = null)
		{
			if(settings == null)
				return this.DequeueOrPeekAsync(0);
			else
				return this.DequeueOrPeekAsync((int)settings.PollingTimeout.TotalSeconds);
		}

		public override async Task<IEnumerable<Zongsoft.Messaging.MessageBase>> DequeueAsync(int count, Zongsoft.Messaging.MessageDequeueSettings settings = null)
		{
			if(count < 1)
				throw new ArgumentOutOfRangeException("count");

			var messages = new List<Zongsoft.Messaging.MessageBase>(count);

			for(int i = 0; i < count; i++)
			{
				var message = await this.DequeueAsync(settings);

				//如果返回的结果为空则表示队列已空
				if(message == null)
					return messages;

				messages.Add(message);
			}

			return messages;
		}

		public override Task<Zongsoft.Messaging.MessageBase> PeekAsync()
		{
			return this.DequeueOrPeekAsync(-1);
		}

		public override async Task<IEnumerable<Zongsoft.Messaging.MessageBase>> PeekAsync(int count)
		{
			if(count > 1)
				throw new ArgumentOutOfRangeException("count");

			return new Zongsoft.Messaging.MessageBase[] { await this.DequeueOrPeekAsync(-1) };
		}

		private async Task<Zongsoft.Messaging.MessageBase> DequeueOrPeekAsync(int waitSeconds)
		{
			var client = this.CreateHttpClient();
			var request = new HttpRequestMessage(HttpMethod.Get, this.GetRequestUrl("messages") + (waitSeconds >= 0 ? "?waitseconds=" + waitSeconds.ToString() : "?peekonly=true"));
			var response = await client.SendAsync(request);

			if(response.IsSuccessStatusCode)
				return MessageUtility.ResolveMessage(this, await response.Content.ReadAsStreamAsync());

			var exception = AliyunException.Parse(await response.Content.ReadAsStringAsync());

			if(exception != null)
			{
				switch(exception.Code)
				{
					case MessageUtility.MessageNotExist:
						return null;
					case MessageUtility.QueueNotExist:
						throw exception;
					default:
						throw exception;
				}
			}

			return null;
		}
		#endregion

		#region 虚拟方法
		protected virtual byte[] SerializeContent(object item)
		{
			if(item == null)
				return null;

			if(item.GetType() == typeof(byte[]))
				return (byte[])item;

			if(item is string)
				return Encoding.UTF8.GetBytes((string)item);

			if(Zongsoft.Common.TypeExtension.IsScalarType(item.GetType()))
				return Encoding.UTF8.GetBytes(item.ToString());

			if(item is System.IO.Stream)
			{
				var stream = (System.IO.Stream)item;
				var buffer = new byte[stream.Length - stream.Position];

				stream.Read(buffer, 0, buffer.Length);
				return buffer;
			}

			if(Zongsoft.Runtime.Serialization.Serializer.CanSerialize(item))
			{
				using(var stream = new MemoryStream())
				{
					if(Zongsoft.Runtime.Serialization.Serializer.TrySerialize(stream, item))
						return stream.ToArray();
				}
			}

			throw new NotSupportedException(string.Format("The '{0}' content of message can not serialize.", item.GetType()));
		}
		#endregion

		#region 内部方法
		internal HttpClient CreateHttpClient()
		{
			if(_provider == null)
				throw new InvalidOperationException("");

			return new HttpClient(new HttpClientHandler(_provider.Certification, MessageQueueAuthenticator.Instance));
		}

		internal string GetRequestUrl(params string[] parts)
		{
			var args = (parts == null ? new string[1] : new string[parts.Length + 1]);
			args[0] = this.Name;

			if(parts != null && parts.Length > 0)
				Array.Copy(parts, 0, args, 1, parts.Length);

			return _provider.GetRequestUrl(args);
		}
		#endregion
	}
}

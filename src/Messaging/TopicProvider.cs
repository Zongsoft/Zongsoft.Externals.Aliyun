/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Net.Http;
using System.Text;

using Zongsoft.Messaging;
using Zongsoft.Communication;

namespace Zongsoft.Externals.Aliyun.Messaging
{
	public class TopicProvider : Zongsoft.Messaging.ITopicProvider
	{
		#region 成员字段
		private HttpClient _http;
		private Options.IConfiguration _configuration;
		private readonly IDictionary<string, ITopic> _topics;
		#endregion

		#region 构造函数
		public TopicProvider()
		{
			_topics = new Dictionary<string, ITopic>();
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置配置信息。
		/// </summary>
		public Options.IConfiguration Configuration
		{
			get
			{
				return _configuration;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_configuration = value;
			}
		}

		public MessageQueueServiceCenter ServiceCenter
		{
			get
			{
				return MessageQueueServiceCenter.GetInstance(_configuration.Name, _configuration.IsInternal);
			}
		}

		public ITopic this[string name]
		{
			get
			{
				if(string.IsNullOrWhiteSpace(name))
					throw new ArgumentNullException("name");

				return this.Get(name);
			}
		}
		#endregion

		#region 保护属性
		public HttpClient Http
		{
			get
			{
				if(_http == null)
				{
					if(_configuration == null)
						throw new InvalidOperationException("Missing required configuration.");

					lock(_configuration)
					{
						if(_http == null)
						{
							_http = new HttpClient(new HttpClientHandler(_configuration.Certification, MessageQueueAuthenticator.Instance));
							_http.DefaultRequestHeaders.Add("x-mns-version", "2015-06-06");
						}
					}
				}

				return _http;
			}
		}
		#endregion

		#region 公共方法
		public ITopic Get(string name)
		{
			if(string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			ITopic topic;

			if(_topics.TryGetValue(name, out topic))
				return topic;

			var http = this.Http;
			var response = http.GetAsync(this.GetRequestUrl(name));

			if(response.Result.IsSuccessStatusCode)
			{
				var info = MessageUtility.ResolveTopicInfo(response.Result.Content.ReadAsStreamAsync().Result);

				if(info != null)
					return new Topic(this, name, info);
			}

			return null;
		}

		public ITopic Register(string name, object state = null)
		{
			if(string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var http = this.Http;

			var response = http.PutAsync(this.GetRequestUrl(name), new StringContent("", Encoding.UTF8, "application/xml"));

			if(response.Result.IsSuccessStatusCode)
			{
				var topic = _topics[name] = new Topic(this, name, null);
				return topic;
			}

			return null;
		}

		public bool Unregister(string name)
		{
			if(string.IsNullOrEmpty(name))
				return false;

			var http = this.Http;
			var response = http.DeleteAsync(this.GetRequestUrl(name));

			return response.Result != null && response.Result.IsSuccessStatusCode;
		}
		#endregion

		#region 内部方法
		internal string GetRequestUrl(params string[] parts)
		{
			var configuration = this.Configuration;

			if(configuration == null)
				throw new InvalidOperationException("Missing required configuration.");

			var path = parts == null ? string.Empty : string.Join("/", parts);

			if(string.IsNullOrEmpty(path))
				return string.Format("http://{0}.{1}/topics", configuration.Messaging.Name, this.ServiceCenter.Path);
			else
				return string.Format("http://{0}.{1}/topics/{2}", configuration.Messaging.Name, this.ServiceCenter.Path, path);
		}
		#endregion
	}
}

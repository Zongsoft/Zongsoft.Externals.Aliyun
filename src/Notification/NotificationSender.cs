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
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Zongsoft.Externals.Aliyun.Notification
{
	/// <summary>
	/// 表示移动推送器的类。
	/// </summary>
	public class NotificationSender
	{
		#region 成员字段
		private readonly object _syncRoot;
		private HttpClient _client;
		private Options.IConfiguration _configuration;
		#endregion

		#region 构造函数
		public NotificationSender()
		{
			_syncRoot = new object();
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

		/// <summary>
		/// 获取当前的服务中心。
		/// </summary>
		public NotificationServiceCenter ServiceCenter
		{
			get
			{
				var configuration = this.Configuration;

				if(configuration == null)
					return null;

				return NotificationServiceCenter.GetInstance(configuration.Name);
			}
		}
		#endregion

		#region 公共方法
		/// <summary>
		/// 发送消息或通知到移动设备。
		/// </summary>
		/// <param name="name">指定的消息推送通道的名字（即阿里云移动推送的AppKey参数）。</param>
		/// <param name="title">指定的消息或通知的标题。</param>
		/// <param name="content">指定的消息或通知的内容。</param>
		/// <param name="destination">指定的推送目标。</param>
		/// <param name="settings">指定的推送设置参数。</param>
		/// <returns>返回推送的结果。</returns>
		public async Task<NotificationResult> Send(string name, string title, string content, string destination, NotificationSenderSettings settings)
		{
			var configuration = this.Configuration;

			if(configuration == null)
				throw new InvalidOperationException("Missing configuration.");

			if(string.IsNullOrWhiteSpace(destination))
				throw new ArgumentNullException(nameof(destination));

			if(string.IsNullOrWhiteSpace(content))
				return null;

			//获取请求的查询参数集
			var parameters = this.GetParameters(name, settings);

			parameters["TargetValue"] = destination;
			parameters["Title"] = string.IsNullOrWhiteSpace(title) ? "New " + settings.Type.ToString() : title;
			parameters["Body"] = content;

			var url = "http://" + this.ServiceCenter.Path + "?" + this.GetQueryString(parameters);
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Accept.TryParseAdd("application/json");

			//var dictionary = new Dictionary<string, string>
			//{
			//	{ "Title", string.IsNullOrWhiteSpace(title) ? "New " + settings.Type.ToString() : title },
			//	{ "Body", content },
			//	{ "TargetValue", destination },
			//};
			//request.Content = new FormUrlEncodedContent(dictionary);
			//request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			var response = await this.GetHttpClient().SendAsync(request);

			if(string.Equals(response.Content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase) ||
			   string.Equals(response.Content.Headers.ContentType.MediaType, "text/json", StringComparison.OrdinalIgnoreCase))
				return Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<NotificationResult>(await response.Content.ReadAsStringAsync());

			return new NotificationResult()
			{
				Code = response.StatusCode.ToString(),
				Message = await response.Content.ReadAsStringAsync(),
			};
		}
		#endregion

		#region 虚拟方法
		protected virtual IDictionary<string, string> GetParameters(string name, NotificationSenderSettings settings)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			var dictionary = new Dictionary<string, string>()
			{
				//推送方式（高级推送）
				{ "Action", "Push" },

				//以下是公共参数部分
				{ "Format", "JSON" },
				{ "RegionId", this.ServiceCenter?.Region },
				{ "Version", "2016-08-01" },
				{ "AccessKeyId", _configuration.Certification.Name },
				{ "SignatureMethod", "HMAC-SHA1" },
				{ "SignatureVersion", "1.0" },
				{ "SignatureNonce", ((ulong)Zongsoft.Common.RandomGenerator.GenerateInt64()).ToString() },
				{ "Timestamp", DateTime.UtcNow.ToString("s") + "Z" },

				//以下是推送接口特定参数
				{ "AppKey", name },
				{ "PushType", settings.Type.ToString().ToUpperInvariant() },
				{ "DeviceType", settings.DeviceType.ToString().ToUpperInvariant() },
				{ "Target", settings.TargetType.ToString().ToUpperInvariant() },
			};

			if(settings.Expiry > 0)
			{
				dictionary["StoreOffline"] = "true";
				dictionary["ExpireTime"] = DateTime.UtcNow.AddMinutes(settings.Expiry).ToString("s") + "Z";
			}

			return dictionary;
		}
		#endregion

		#region 私有方法
		private string GetQueryString(IDictionary<string, string> parameters)
		{
			if(parameters == null || parameters.Count == 0)
				return string.Empty;

			var text = new System.Text.StringBuilder();

			foreach(var parameter in parameters)
			{
				if(text.Length > 0)
					text.Append("&");

				text.Append(parameter.Key + "=" + parameter.Value);
			}

			return text.ToString();
		}

		private HttpClient GetHttpClient()
		{
			if(_client == null)
			{
				lock(_syncRoot)
				{
					if(_client == null)
						_client = new HttpClient(new HttpClientHandler(_configuration.Certification, NotificationAuthenticator.Instance));
				}
			}

			return _client;
		}
		#endregion
	}
}

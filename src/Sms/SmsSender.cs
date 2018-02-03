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
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace Zongsoft.Externals.Aliyun.Sms
{
	/// <summary>
	/// 提供手机短信发送功能的类。
	/// </summary>
	public class SmsSender
	{
		#region 成员字段
		private Options.IConfiguration _configuration;
		private readonly ConcurrentDictionary<string, HttpClient> _pool;
		#endregion

		#region 构造函数
		public SmsSender()
		{
			_pool = new ConcurrentDictionary<string, HttpClient>();
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置手机短信配置信息。
		/// </summary>
		public Options.IConfiguration Configuration
		{
			get
			{
				return _configuration;
			}
			set
			{
				_configuration = value ?? throw new ArgumentNullException();
			}
		}
		#endregion

		#region 公共方法
		/// <summary>
		/// 发送短信到指定的手机号，支持多发。
		/// </summary>
		/// <param name="name">指定的短信模板名称。</param>
		/// <param name="destination">目标手机号码集。</param>
		/// <param name="parameter">短信模板参数对象。</param>
		/// <param name="extra">扩展附加数据，通常表示特定的业务数据。</param>
		/// <returns>返回的短信发送结果信息。</returns>
		public async Task<SmsSendResult> SendAsync(string name, IEnumerable<string> destination, object parameter, string extra = null)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			if(destination == null)
				throw new ArgumentNullException(nameof(destination));

			var target = string.Join(",", destination);

			if(string.IsNullOrWhiteSpace(target))
				throw new InvalidOperationException("Missing destination for the send of sms.");

			//确认当前短信功能的配置
			var configuration = this.EnsureConfiguration();

			//获取指定名称的短信模板配置，如果获取失败则抛出异常
			if(!configuration.Templates.TryGet(name, out var template))
				throw new InvalidOperationException($"The specified '{name}' sms template is not existed.");

			//获取当前短信模板关联的凭证
			var certificate = this.GetCertificate(template);

			//生成请求的查询参数集
			var parameters = this.GenerateParameters(template, target);

			if(parameter != null)
			{
				if(parameter is string || parameter is System.Text.StringBuilder)
					parameters.Add("TemplateParam", parameter.ToString());
				else
					parameters.Add("TemplateParam", Runtime.Serialization.Serializer.Json.Serialize(parameter));
			}

			if(!string.IsNullOrWhiteSpace(extra))
				parameters.Add("OutId", extra.Trim());

			//获取当前短信模板所在的服务区域
			var center = SmsServiceCenter.GetInstance(this.GetRegion(template));

			//构建短信发送的HTTP请求消息包
			var request = new HttpRequestMessage(HttpMethod.Get, "http://" + center.Path + "?" + Utility.GetQueryString(parameters));
			request.Headers.Accept.TryParseAdd("application/json");

			//获取当前实例关联的HTTP客户端程序
			var http = this.GetHttpClient(certificate);

			//提交短信发送请求
			var response = await http.SendAsync(request);

			return await this.GetResultAsync(response.Content);
		}
		#endregion

		#region 虚拟方法
		protected virtual IDictionary<string, string> GenerateParameters(Options.ITemplateOption template, string target)
		{
			var center = SmsServiceCenter.GetInstance(this.GetRegion(template));

			var dictionary = new Dictionary<string, string>()
			{
				//操作名称（发送短信）
				{ "Action", "SendSms" },

				//以下是公共参数部分
				{ "Format", "JSON" },
				{ "RegionId", center.Alias },
				{ "Version", "2017-05-25" },
				{ "AccessKeyId", this.GetCertificate(template).Name },
				{ "SignatureMethod", "HMAC-SHA1" },
				{ "SignatureVersion", "1.0" },
				{ "SignatureNonce", Guid.NewGuid().ToString("N") },
				{ "Timestamp", Utility.GetTimestamp() },

				//以下是短信接口特定参数
				{ "TemplateCode", template.Code },
				{ "SignName", template.Scheme },
				{ "PhoneNumbers", target },
			};

			return dictionary;
		}
		#endregion

		#region 私有方法
		private ICertificate GetCertificate(Options.ITemplateOption template)
		{
			var certificate = template?.Certificate;

			if(string.IsNullOrWhiteSpace(certificate))
				certificate = _configuration?.Certificate;

			if(string.IsNullOrWhiteSpace(certificate))
				return Aliyun.Configuration.Instance.Certificates.Default;

			return Aliyun.Configuration.Instance.Certificates.Get(certificate);
		}

		private ServiceCenterName GetRegion(Options.ITemplateOption template)
		{
			return template?.Region ?? _configuration?.Region ?? Aliyun.Configuration.Instance.Name;
		}

		private HttpClient GetHttpClient(ICertificate certificate)
		{
			return _pool.GetOrAdd(certificate.Name, key =>
			{
				if(certificate == null)
					throw new ArgumentNullException(nameof(certificate));

				var http = new HttpClient(new HttpClientHandler(certificate, SmsAuthenticator.Instance));

				//尝试构建固定的请求头
				//http.DefaultRequestHeaders.TryAddWithoutValidation("SignatureMethod", "HMAC-SHA1");
				//http.DefaultRequestHeaders.TryAddWithoutValidation("SignatureVersion", "1.0");
				//http.DefaultRequestHeaders.TryAddWithoutValidation("Format", "JSON");
				//http.DefaultRequestHeaders.TryAddWithoutValidation("Action", "SendSms");
				//http.DefaultRequestHeaders.TryAddWithoutValidation("Version", "2017-05-25");

				return http;
			});
		}

		private async Task<SmsSendResult> GetResultAsync(HttpContent content)
		{
			var text = await content.ReadAsStringAsync();

			if(string.Equals(content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase) ||
			   string.Equals(content.Headers.ContentType.MediaType, "text/json", StringComparison.OrdinalIgnoreCase))
				return Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<SmsSendResult>(text);

			return new SmsSendResult("Unknown", text);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private Options.IConfiguration EnsureConfiguration()
		{
			return this.Configuration ?? throw new InvalidOperationException("Missing required configuration of the sms sender(aliyun).");
		}
		#endregion
	}
}

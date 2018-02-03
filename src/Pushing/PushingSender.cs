﻿/*
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

namespace Zongsoft.Externals.Aliyun.Pushing
{
	/// <summary>
	/// 提供移动推送功能的类。
	/// </summary>
	public class PushingSender
	{
		#region 成员字段
		private Options.IConfiguration _configuration;
		private readonly ConcurrentDictionary<string, HttpClient> _pool;
		#endregion

		#region 构造函数
		public PushingSender()
		{
			_pool = new ConcurrentDictionary<string, HttpClient>();
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置移动推送配置信息。
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
		/// 发送消息或通知到移动设备。
		/// </summary>
		/// <param name="name">指定的消息推送应用的名字。</param>
		/// <param name="title">指定的消息或通知的标题。</param>
		/// <param name="content">指定的消息或通知的内容。</param>
		/// <param name="destination">指定的推送目标。</param>
		/// <param name="settings">指定的推送设置参数。</param>
		/// <returns>返回推送的结果。</returns>
		public async Task<PushingResult> SendAsync(string name, string title, string content, string destination, PushingSenderSettings settings)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			if(string.IsNullOrWhiteSpace(destination))
				throw new ArgumentNullException(nameof(destination));

			if(string.IsNullOrWhiteSpace(content))
				return null;

			//确认移动推送的配置
			var configuration = this.EnsureConfiguration();

			//获取指定名称的短信模板配置，如果获取失败则抛出异常
			if(!configuration.Apps.TryGet(name, out var app))
				throw new InvalidOperationException($"The specified '{name}' app is not existed.");

			//获取当前短信模板关联的凭证
			var certificate = this.GetCertificate(app);

			//生成请求的查询参数集
			var parameters = this.GenerateParameters(app, settings);

			parameters["TargetValue"] = destination;
			parameters["Title"] = string.IsNullOrWhiteSpace(title) ? "New " + settings.Type.ToString() : title;
			parameters["Body"] = content;

			//获取当前应用所在的服务区域
			var center = PushingServiceCenter.GetInstance(this.GetRegion(app));

			//构建移动推送的HTTP请求消息包
			var request = new HttpRequestMessage(HttpMethod.Get, "http://" + center.Path + "?" + Utility.GetQueryString(parameters));
			request.Headers.Accept.TryParseAdd("application/json");

			//获取当前实例关联的HTTP客户端程序
			var http = this.GetHttpClient(certificate);

			//提交移动推送请求
			var response = await http.SendAsync(request);

			return await this.GetResultAsync(response.Content);
		}
		#endregion

		#region 虚拟方法
		protected virtual IDictionary<string, string> GenerateParameters(Options.IAppOption app, PushingSenderSettings settings)
		{
			var center = PushingServiceCenter.GetInstance(this.GetRegion(app));

			var dictionary = new Dictionary<string, string>()
			{
				//推送方式（高级推送）
				{ "Action", "Push" },

				//以下是公共参数部分
				{ "Format", "JSON" },
				{ "RegionId", center.Alias },
				{ "Version", "2016-08-01" },
				{ "AccessKeyId", this.GetCertificate(app).Name },
				{ "SignatureMethod", "HMAC-SHA1" },
				{ "SignatureVersion", "1.0" },
				{ "SignatureNonce", ((ulong)Zongsoft.Common.RandomGenerator.GenerateInt64()).ToString() },
				{ "Timestamp", Utility.GetTimestamp() },

				//以下是推送接口特定参数
				{ "AppKey", app.Code },
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
		private ICertificate GetCertificate(Options.IAppOption app)
		{
			var certificate = app?.Certificate;

			if(string.IsNullOrWhiteSpace(certificate))
				certificate = _configuration?.Certificate;

			if(string.IsNullOrWhiteSpace(certificate))
				return Aliyun.Configuration.Instance.Certificates.Default;

			return Aliyun.Configuration.Instance.Certificates.Get(certificate);
		}

		private ServiceCenterName GetRegion(Options.IAppOption app)
		{
			return app?.Region ?? _configuration?.Region ?? Aliyun.Configuration.Instance.Name;
		}

		private HttpClient GetHttpClient(ICertificate certificate)
		{
			return _pool.GetOrAdd(certificate.Name, key =>
			{
				if(certificate == null)
					throw new ArgumentNullException(nameof(certificate));

				var http = new HttpClient(new HttpClientHandler(certificate, PushingAuthenticator.Instance));

				//尝试构建固定的请求头
				http.DefaultRequestHeaders.TryAddWithoutValidation("SignatureMethod", "HMAC-SHA1");
				http.DefaultRequestHeaders.TryAddWithoutValidation("SignatureVersion", "1.0");
				http.DefaultRequestHeaders.TryAddWithoutValidation("Format", "JSON");
				http.DefaultRequestHeaders.TryAddWithoutValidation("Action", "Push");
				http.DefaultRequestHeaders.TryAddWithoutValidation("Version", "2016-08-01");

				return http;
			});
		}

		private async Task<PushingResult> GetResultAsync(HttpContent content)
		{
			var text = await content.ReadAsStringAsync();

			if(string.Equals(content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase) ||
			   string.Equals(content.Headers.ContentType.MediaType, "text/json", StringComparison.OrdinalIgnoreCase))
				return Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<PushingResult>(text);

			return new PushingResult("Unknown", text);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private Options.IConfiguration EnsureConfiguration()
		{
			return this.Configuration ?? throw new InvalidOperationException("Missing required configuration of the mobile-pushing sender(aliyun).");
		}
		#endregion
	}
}

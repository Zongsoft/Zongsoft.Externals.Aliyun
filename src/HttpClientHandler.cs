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
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Zongsoft.Externals.Aliyun
{
	internal class HttpClientHandler : System.Net.Http.HttpClientHandler
	{
		#region 成员字段
		private ICertification _certification;
		private HttpAuthenticator _authenticator;
		#endregion

		#region 构造函数
		public HttpClientHandler(ICertification certification, HttpAuthenticator authenticator)
		{
			if(certification == null)
				throw new ArgumentNullException("certification");

			if(authenticator == null)
				throw new ArgumentNullException("authenticator");

			_certification = certification;
			_authenticator = authenticator;
		}
		#endregion

		#region 重写方法
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.Headers.Date = DateTime.UtcNow;

			switch(_authenticator.SignatureMode)
			{
				case HttpSignatureMode.Header:
					request.Headers.Authorization = new AuthenticationHeaderValue(_authenticator.Name, _certification.Name + ":" + _authenticator.Signature(request, _certification.Secret));
					break;
				case HttpSignatureMode.Parameter:
					var delimiter = string.IsNullOrWhiteSpace(request.RequestUri.Query) ? "?" : "&";

					request.RequestUri = new Uri(
						request.RequestUri.Scheme + "://" +
						request.RequestUri.Authority +
						request.RequestUri.PathAndQuery + delimiter +
						_authenticator.Name + "="  + _authenticator.Signature(request, _certification.Secret) +
						request.RequestUri.Fragment);

					break;
			}

			return base.SendAsync(request, cancellationToken);
		}
		#endregion
	}
}

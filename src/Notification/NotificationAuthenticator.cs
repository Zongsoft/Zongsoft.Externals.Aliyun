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

namespace Zongsoft.Externals.Aliyun.Notification
{
	public class NotificationAuthenticator : HttpAuthenticator
	{
		#region 单例字段
		public static NotificationAuthenticator Instance = new NotificationAuthenticator();
		#endregion

		#region 私有构造
		private NotificationAuthenticator() : base("Signature", HttpSignatureMode.Parameter)
		{
		}
		#endregion

		#region 重写方法
		public override string Signature(HttpRequestMessage request, string secret)
		{
			return base.Signature(request, secret + "&");
		}

		protected override string Canonicalize(HttpRequestMessage request)
		{
			var canonicalizedString = base.Canonicalize(request);

			return request.Method.Method + "&%2F&" + Uri.EscapeDataString(canonicalizedString);
		}

		protected override string CanonicalizeHeaders(HttpRequestMessage request)
		{
			return null;
		}

		protected override string CanonicalizeResource(HttpRequestMessage request)
		{
			var parts = request.RequestUri.Query.TrimStart('?').Split('&');
			var dictionary = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach(var part in parts)
			{
				var index = part.IndexOf('=');

				if(index > 0)
					dictionary[part.Substring(0, index)] = index < part.Length - 1 ? part.Substring(index + 1) : null;
				else
					dictionary[part] = null;
			}

			var text = new StringBuilder((int)Math.Ceiling(request.RequestUri.Query.Length * 1.5));

			foreach(var entry in dictionary)
			{
				if(text.Length > 0)
					text.Append("&");

				text.Append(this.CanonicalizeString(entry.Key) + "=" + this.CanonicalizeString(entry.Value));
			}

			return text.Replace("%2A", "*").Replace("%7E", "~").ToString();
		}

		protected override bool IsCanonicalizedHeader(string name)
		{
			return false;
		}
		#endregion

		#region 私有方法
		private string CanonicalizeString(string value)
		{
			return Uri.EscapeDataString(value);
		}
		#endregion
	}
}

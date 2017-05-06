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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Zongsoft.Externals.Aliyun
{
	public class HttpAuthenticator
	{
		#region 常量定义
		public const string NewLine = "\n";
		#endregion

		#region 成员字段
		private string _name;
		private HttpSignatureMode _signatureMode;
		#endregion

		#region 构造函数
		protected HttpAuthenticator(string name, HttpSignatureMode signatureMode)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			_name = name.Trim();
			_signatureMode = signatureMode;
		}
		#endregion

		#region 公共属性
		public string Name
		{
			get
			{
				return _name;
			}
		}

		public HttpSignatureMode SignatureMode
		{
			get
			{
				return _signatureMode;
			}
		}
		#endregion

		#region 公共方法
		public virtual string Signature(HttpRequestMessage request, string secret)
		{
			using(var algorithm = HMACSHA1.Create())
			{
				//设置散列加密算法的密钥
				algorithm.Key = Encoding.UTF8.GetBytes(secret);

				//计算当前请求的签名数据
				var data = Encoding.UTF8.GetBytes(Canonicalize(request));

				//计算加密后的散列值（签名内容）
				return System.Convert.ToBase64String(algorithm.ComputeHash(data));
			}
		}
		#endregion

		#region 虚拟方法
		protected virtual string Canonicalize(HttpRequestMessage request)
		{
			if(request == null)
				throw new ArgumentNullException("request");

			var headersString = this.CanonicalizeHeaders(request);
			var resourceString = this.CanonicalizeResource(request);

			return headersString + resourceString;
		}

		protected virtual string CanonicalizeHeaders(HttpRequestMessage request)
		{
			var text = new StringBuilder(request.Method.ToString().ToUpperInvariant() + NewLine, 512);

			if(request.Content != null && request.Content.Headers != null && (request.Content.Headers.ContentMD5 != null && request.Content.Headers.ContentMD5.Length > 0))
				text.Append(System.Convert.ToBase64String(request.Content.Headers.ContentMD5) + NewLine);
			else
				text.Append(NewLine);

			if(request.Content != null && request.Content.Headers != null && (request.Content.Headers.ContentType != null && !string.IsNullOrWhiteSpace(request.Content.Headers.ContentType.MediaType)))
				text.Append(request.Content.Headers.ContentType.ToString() + NewLine);
			else
				text.Append(NewLine);

			if(request.Headers.Date == null)
				request.Headers.Date = DateTime.UtcNow;

			text.Append(request.Headers.Date.Value.ToString("r") + NewLine);

			var dictionary = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach(KeyValuePair<string, IEnumerable<string>> header in request.Headers)
			{
				if(this.IsCanonicalizedHeader(header.Key))
				{
					string value, key = header.Key.ToLowerInvariant().Trim();

					if(dictionary.TryGetValue(key, out value))
						dictionary[key] = JoinValues(value, header.Value);
					else
						dictionary[key] = JoinValues(null, header.Value);
				}
			}

			foreach(var entry in dictionary)
			{
				text.AppendFormat("{0}:{1}{2}", entry.Key, entry.Value, NewLine);
			}

			return text.ToString();
		}

		protected virtual string CanonicalizeResource(HttpRequestMessage request)
		{
			return null;
		}

		protected virtual bool IsCanonicalizedHeader(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
				return false;

			return name.StartsWith("x-");
		}
		#endregion

		#region 私有方法
		private string JoinValues(string originalValue, IEnumerable<string> values)
		{
			if(values == null)
				return originalValue;

			var result = string.Join(",", values).Trim().Trim(',');

			if(!string.IsNullOrWhiteSpace(originalValue))
				result = originalValue.Trim() + ',' + result;

			return result;
		}
		#endregion

		#region 嵌套子类
		protected class QueryStringComparer : IComparer<string>
		{
			#region 单例字段
			public static readonly QueryStringComparer Ordinal = new QueryStringComparer();
			#endregion

			#region 私有构造
			private QueryStringComparer()
			{
			}
			#endregion

			#region 公共方法
			public int Compare(string x, string y)
			{
				if(string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y))
					return 0;

				if(string.IsNullOrEmpty(x))
					return -1;

				if(string.IsNullOrEmpty(y))
					return 1;

				for(int i = 0; i < Math.Min(x.Length, y.Length); i++)
				{
					var xv = GetCharNumber(x[i]);
					var yv = GetCharNumber(y[i]);

					if(xv != yv)
						return xv < yv ? -1 : 1;
				}

				if(x.Length == y.Length)
					return 0;

				return x.Length < y.Length ? -1 : 1;
			}
			#endregion

			#region 私有方法
			private int GetCharNumber(Char chr)
			{
				if(chr >= '0' && chr <= '9')
					return chr + 7;

				if(chr > '9' && chr < 'A')
					return chr - 10;

				return chr;
			}
			#endregion
		}
		#endregion
	}
}

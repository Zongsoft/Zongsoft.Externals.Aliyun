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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml;

namespace Zongsoft.Externals.Aliyun.Storages
{
	internal class StorageSearchResultResolver : IDisposable
	{
		#region 成员字段
		private HttpClient _client;
		private StorageServiceCenter _serviceCenter;
		private Func<string, string> _getUrl;
		#endregion

		#region 构造函数
		public StorageSearchResultResolver(StorageServiceCenter serviceCenter, HttpClient client, Func<string, string> getUrl = null)
		{
			if(serviceCenter == null)
				throw new ArgumentNullException("serviceCenter");

			if(client == null)
				throw new ArgumentNullException("client");

			_client = client;
			_serviceCenter = serviceCenter;
			_getUrl = getUrl;
		}
		#endregion

		#region 公共属性
		public HttpClient Client
		{
			get
			{
				return _client;
			}
		}

		public StorageServiceCenter ServiceCenter
		{
			get
			{
				return _serviceCenter;
			}
		}

		internal Func<string, string> UrlThunk
		{
			get
			{
				return _getUrl;
			}
		}
		#endregion

		#region 解析方法
		public async Task<StorageSearchResult> Resolve(HttpResponseMessage response)
		{
			if(response == null)
				throw new ArgumentNullException("response");

			if(!response.IsSuccessStatusCode)
				return null;

			if(response.Content.Headers.ContentLength == null || response.Content.Headers.ContentLength < 1)
				return null;

			StorageSearchResult result = null;
			var contentStream = await response.Content.ReadAsStreamAsync();

			this.ResolveCore(contentStream, (bucketName, pattern, marker) =>
			{
				result = new StorageSearchResult(bucketName, pattern, marker, this);
				return result;
			});

			return result;
		}

		public string Reload(StorageSearchResult owner, HttpResponseMessage response)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			if(response == null)
				throw new ArgumentNullException("response");

			if(!response.IsSuccessStatusCode)
				return null;

			if(response.Content.Headers.ContentLength == null || response.Content.Headers.ContentLength < 1)
				return null;

			return this.ResolveCore(response.Content.ReadAsStreamAsync().Result, (_, __, ___) => owner);
		}

		private string ResolveCore(Stream stream, Func<string, string, string, StorageSearchResult> thunk)
		{
			if(stream == null)
				return null;

			var settings = new XmlReaderSettings()
			{
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true,
			};

			StorageSearchResult owner = null;
			string name = null, pattern = null, marker = null;

			using(var reader = XmlReader.Create(stream, settings))
			{
				if(reader.MoveToContent() != XmlNodeType.Element)
					return null;

				while(reader.Read())
				{
					if(reader.NodeType != XmlNodeType.Element)
						continue;

					switch(reader.LocalName)
					{
						case "Name":
							name = Utility.Xml.ReadContentAsString(reader);
							break;
						case "Prefix":
							pattern = Utility.Xml.ReadContentAsString(reader);
							break;
						case "NextMarker":
							marker = Utility.Xml.ReadContentAsString(reader);
							break;
						case "Contents":
							var dictionary = this.ResolveContent(reader);

							if(dictionary != null && dictionary.Count > 0)
							{
								if(owner == null)
									owner = thunk(name, pattern, marker);

								owner.Append(dictionary["Key"],
									Zongsoft.Common.Convert.ConvertValue<long>(dictionary["Size"]),
									dictionary["ETag"],
									Zongsoft.Common.Convert.ConvertValue<DateTimeOffset>(dictionary["LastModified"]));
							}

							break;
						case "CommonPrefixes":
							var prefix = this.ResolveCommonPrefix(reader);

							if(!string.IsNullOrWhiteSpace(prefix))
							{
								if(owner == null)
									owner = thunk(name, pattern, marker);

								owner.Append(prefix, 0, null, DateTimeOffset.MinValue);
							}

							break;
					}
				}
			}

			return marker;
		}
		#endregion

		#region 私有方法
		private IDictionary<string, string> ResolveContent(XmlReader reader)
		{
			if(reader.ReadState == ReadState.Initial)
				reader.Read();

			if(reader.LocalName != "Contents")
				return null;

			var depth = reader.Depth;
			var result = new Dictionary<string, string>();

			while(reader.Read() && reader.Depth > depth)
			{
				if(reader.NodeType != XmlNodeType.Element)
					continue;

				switch(reader.LocalName)
				{
					case "Key":
					case "ETag":
					case "Size":
					case "LastModified":
						result[reader.LocalName] = Utility.Xml.ReadContentAsString(reader);
						break;
					case "Owner":
						Utility.Xml.MoveToEndElement(reader);
						break;
					case "Type":
					case "StorageClass":
						break;
				}
			}

			return result;
		}

		private string ResolveCommonPrefix(XmlReader reader)
		{
			if(reader.ReadState == ReadState.Initial)
				reader.Read();

			if(reader.LocalName != "CommonPrefixes")
				return null;

			var depth = reader.Depth;

			if(reader.Read() && reader.Depth > depth)
			{
				if(reader.NodeType == XmlNodeType.Element && reader.LocalName == "Prefix")
					return Utility.Xml.ReadContentAsString(reader);
			}

			return null;
		}
		#endregion

		#region 释放资源
		public void Dispose()
		{
			var client = System.Threading.Interlocked.Exchange(ref _client, null);

			if(client != null)
				client.Dispose();
		}
		#endregion
	}
}

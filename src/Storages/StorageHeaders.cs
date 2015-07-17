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
using System.Threading.Tasks;

namespace Zongsoft.Externals.Aliyun.Storages
{
	internal class StorageHeaders
	{
		public const string OSS_PREFIX = "x-oss-";
		public const string OSS_META = OSS_PREFIX + "meta-";
		public const string OSS_COPY_SOURCE = OSS_PREFIX + "copy-source";
		public const string OSS_COPY_DIRECTIVE = OSS_PREFIX + "metadata-directive";

		//自定义扩展属性常量
		public const string ZFS_CREATEDTIME_PROPERTY = "CreatedTime";

		//标准的HTTP头的常量
		public const string HTTP_ETAG_PROPERTY = "HTTP:ETag";
		public const string HTTP_CONTENT_LENGTH_PROPERTY = "HTTP:Content-Length";
		public const string HTTP_LAST_MODIFIED_PROPERTY = "HTTP:Last-Modified";
	}
}

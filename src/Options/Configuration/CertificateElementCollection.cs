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

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Aliyun.Options.Configuration
{
	public class CertificateElementCollection : OptionConfigurationElementCollection<CertificateElement, ICertificate>, ICertificateProvider
	{
		#region 常量定义
		private const string XML_DEFAULT_ATTRIBUTE = "default";
		private const string XML_CERTIFICATE_ELEMENT = "certificate";
		#endregion

		#region 公共属性
		[OptionConfigurationProperty(XML_DEFAULT_ATTRIBUTE, OptionConfigurationPropertyBehavior.IsRequired)]
		public string Default
		{
			get
			{
				return (string)this.GetAttributeValue(XML_DEFAULT_ATTRIBUTE);
			}
			set
			{
				this.SetAttributeValue(XML_DEFAULT_ATTRIBUTE, value);
			}
		}
		#endregion

		#region 重写方法
		protected override string ElementName
		{
			get
			{
				return XML_CERTIFICATE_ELEMENT;
			}
		}

		protected override string GetElementKey(OptionConfigurationElement element)
		{
			return ((CertificateElement)element).Name;
		}
		#endregion

		#region 接口实现
		ICertificate ICertificateProvider.Default
		{
			get
			{
				var name = (string)this.GetAttributeValue(XML_DEFAULT_ATTRIBUTE);

				if(string.IsNullOrEmpty(name))
					return null;

				return (ICertificate)this.GetElement(name);
			}
		}
		#endregion
	}
}

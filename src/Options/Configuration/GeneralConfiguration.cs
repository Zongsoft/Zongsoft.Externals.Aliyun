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

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Aliyun.Options.Configuration
{
	public class GeneralConfiguration : OptionConfigurationElement, IConfiguration
	{
		#region 常量定义
		private const string XML_NAME_ATTRIBUTE = "name";
		private const string XML_INTERNAL_ATTRIBUTE = "internal";
		private const string XML_CERTIFICATION_ELEMENT = "certification";
		private const string XML_MESSAGING_ELEMENT = "messaging";
		private const string XML_SETTINGS_COLLECTION = "settings";
		#endregion

		#region 公共属性
		[OptionConfigurationProperty(XML_NAME_ATTRIBUTE, Behavior = OptionConfigurationPropertyBehavior.IsRequired)]
		public ServiceCenterName Name
		{
			get
			{
				return (ServiceCenterName)this[XML_NAME_ATTRIBUTE];
			}
			set
			{
				this[XML_NAME_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_INTERNAL_ATTRIBUTE, DefaultValue = true)]
		public bool IsInternal
		{
			get
			{
				return (bool)this[XML_INTERNAL_ATTRIBUTE];
			}
			set
			{
				this[XML_INTERNAL_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_CERTIFICATION_ELEMENT, Type = typeof(CertificationConfigurationElement))]
		public ICertification Certification
		{
			get
			{
				return (ICertification)this[XML_CERTIFICATION_ELEMENT];
			}
		}

		[OptionConfigurationProperty(XML_MESSAGING_ELEMENT)]
		public MessagingConfigurationElement Messaging
		{
			get
			{
				return (MessagingConfigurationElement)this[XML_MESSAGING_ELEMENT];
			}
		}

		[OptionConfigurationProperty(XML_SETTINGS_COLLECTION)]
		public Zongsoft.Options.Configuration.SettingElementCollection Settings
		{
			get
			{
				return (SettingElementCollection)this[XML_SETTINGS_COLLECTION];
			}
		}
		#endregion
	}
}

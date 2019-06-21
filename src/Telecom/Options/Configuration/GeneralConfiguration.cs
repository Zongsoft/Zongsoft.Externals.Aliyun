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
using System.Globalization;
using System.ComponentModel;

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Aliyun.Telecom.Options.Configuration
{
	public class GeneralConfiguration : OptionConfigurationElement, IConfiguration
	{
		#region 常量定义
		private const string XML_REGION_ATTRIBUTE = "region";
		private const string XML_CERTIFICATE_ATTRIBUTE = "certificate";
		private const string XML_MESSAGE_ELEMENT = "message";
		private const string XML_VOICE_ELEMENT = "voice";
		#endregion

		#region 公共属性
		[OptionConfigurationProperty(XML_REGION_ATTRIBUTE, null)]
		public ServiceCenterName? Region
		{
			get
			{
				return (ServiceCenterName?)this[XML_REGION_ATTRIBUTE];
			}
			set
			{
				this[XML_REGION_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_CERTIFICATE_ATTRIBUTE)]
		public string Certificate
		{
			get
			{
				return (string)this[XML_CERTIFICATE_ATTRIBUTE];
			}
			set
			{
				this[XML_CERTIFICATE_ATTRIBUTE] = value;
			}
		}

		[OptionConfigurationProperty(XML_MESSAGE_ELEMENT, Type = typeof(MessageOption))]
		public ITelecomMessageOption Message
		{
			get
			{
				return (ITelecomMessageOption)this[XML_MESSAGE_ELEMENT];
			}
		}

		[OptionConfigurationProperty(XML_VOICE_ELEMENT, Type = typeof(VoiceOption))]
		public ITelecomVoiceOption Voice
		{
			get
			{
				return (ITelecomVoiceOption)this[XML_VOICE_ELEMENT];
			}
		}
		#endregion

		#region 嵌套子类
		private class MessageOption : OptionConfigurationElement, ITelecomMessageOption
		{
			[OptionConfigurationProperty("", Type = typeof(TemplateElementCollection))]
			public Collections.INamedCollection<ITemplateOption> Templates
			{
				get
				{
					return (Collections.INamedCollection<ITemplateOption>)this[string.Empty];
				}
			}
		}

		private class VoiceOption : OptionConfigurationElement, ITelecomVoiceOption
		{
			#region 常量定义
			private const string XML_NUMBERS_ATTRIBUTE = "numbers";
			#endregion

			#region 公共属性
			[TypeConverter(typeof(StringArrayConverter))]
			[OptionConfigurationProperty(XML_NUMBERS_ATTRIBUTE, OptionConfigurationPropertyBehavior.IsRequired)]
			public string[] Numbers
			{
				get
				{
					return (string[])this[XML_NUMBERS_ATTRIBUTE];
				}
				set
				{
					this[XML_NUMBERS_ATTRIBUTE] = value;
				}
			}

			[OptionConfigurationProperty("", Type = typeof(TemplateElementCollection))]
			public Collections.INamedCollection<ITemplateOption> Templates
			{
				get
				{
					return (Collections.INamedCollection<ITemplateOption>)this[string.Empty];
				}
			}
			#endregion

			#region 嵌套子类
			public class StringArrayConverter : System.ComponentModel.TypeConverter
			{
				public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				{
					if(sourceType == typeof(string))
						return true;

					return base.CanConvertFrom(context, sourceType);
				}

				public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				{
					if(destinationType == typeof(string))
						return true;

					return base.CanConvertTo(context, destinationType);
				}

				public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
				{
					if(value == null)
						return null;

					if(value is string text)
					{
						if(string.IsNullOrEmpty(text))
							return null;

						return text.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
					}

					return base.ConvertFrom(context, culture, value);
				}

				public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
				{
					if(value == null)
						return null;

					if(value is string[] array)
					{
						if(array.Length == 0)
							return null;

						return string.Join(",", array);
					}

					return base.ConvertTo(context, culture, value, destinationType);
				}
			}
			#endregion
		}
		#endregion
	}
}

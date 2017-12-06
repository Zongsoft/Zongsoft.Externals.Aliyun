﻿/*
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Zongsoft.Externals.Aliyun.Messaging
{
	internal static class MessageUtility
	{
		#region 常量定义
		public const string QueueNotExist = "QueueNotExist";
		public const string MessageNotExist = "MessageNotExist";

		private static readonly Regex _error_regex = new Regex(@"\<(?'tag'(Code|Message))\>\s*(?<value>[^<>]+)\s*\<\/\k'tag'\>", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
		#endregion

		public static Message ResolveMessage(MessageQueue queue, Stream stream)
		{
			if(stream == null)
				return null;

			string id = null, ackId = null, md5 = null, body = null;
			DateTime? expires = null, enqueuedTime = null, dequeuedTime = null;
			int dequeuedCount = 0;
			byte priority = 0;

			var settings = new XmlReaderSettings()
			{
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true,
			};

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
						case "MessageId":
							id = Utility.Xml.ReadContentAsString(reader);
							break;
						case "ReceiptHandle":
							ackId = Utility.Xml.ReadContentAsString(reader);
							break;
						case "MessageBodyMD5":
							md5 = Utility.Xml.ReadContentAsString(reader);
							break;
						case "MessageBody":
							body = Utility.Xml.ReadContentAsString(reader);
							break;
						case "EnqueueTime":
							enqueuedTime = Utility.GetDateTimeFromEpoch(Utility.Xml.ReadContentAsString(reader));
							break;
						case "NextVisibleTime":
							expires = Utility.GetDateTimeFromEpoch(Utility.Xml.ReadContentAsString(reader));
							break;
						case "FirstDequeueTime":
							dequeuedTime = Utility.GetDateTimeFromEpoch(Utility.Xml.ReadContentAsString(reader));
							break;
						case "DequeueCount":
							dequeuedCount = Zongsoft.Common.Convert.ConvertValue<int>(Utility.Xml.ReadContentAsString(reader));
							break;
						case "Priority":
							priority = Zongsoft.Common.Convert.ConvertValue<byte>(Utility.Xml.ReadContentAsString(reader));
							break;
					}
				}
			}

			if(string.IsNullOrWhiteSpace(id))
				return null;

			return new Message(queue, id,
				string.IsNullOrWhiteSpace(body) ? null : System.Convert.FromBase64String(body),
				string.IsNullOrWhiteSpace(md5) ? null : Zongsoft.Common.Convert.FromHexString(md5),
				expires, enqueuedTime, dequeuedTime, dequeuedCount)
				{
					InnerAcknowledgementId = ackId,
					InnerPriority = priority,
				};
		}

		public static TopicInfo ResolveTopicInfo(Stream stream)
		{
			if(stream == null)
				return null;

			var settings = new XmlReaderSettings()
			{
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true,
			};

			using(var reader = XmlReader.Create(stream, settings))
			{
				if(reader.MoveToContent() != XmlNodeType.Element)
					return null;

				var info = new TopicInfo();

				while(reader.Read())
				{
					if(reader.NodeType != XmlNodeType.Element)
						continue;

					switch(reader.LocalName)
					{
						case "TopicName":
							info.Name = Utility.Xml.ReadContentAsString(reader);
							break;
						case "CreateTime":
							info.CreatedTime = Utility.GetDateTimeFromEpoch(Utility.Xml.ReadContentAsString(reader));
							break;
						case "LastModifyTime":
							info.ModifiedTime = Utility.GetDateTimeFromEpoch(Utility.Xml.ReadContentAsString(reader));
							break;
						case "MessageRetentionPeriod":
							info.MessageRetentionPeriod = TimeSpan.FromSeconds(reader.ReadElementContentAsInt());
							break;
						case "MessageCount":
							info.MessageCount = Zongsoft.Common.Convert.ConvertValue<int>(Utility.Xml.ReadContentAsString(reader));
							break;
						case "LoggingEnabled":
							info.LoggingEnabled = Zongsoft.Common.Convert.ConvertValue<bool>(Utility.Xml.ReadContentAsString(reader));
							break;
					}
				}

				return info;
			}
		}
	}
}

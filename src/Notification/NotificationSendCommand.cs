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

using Zongsoft.Services;
using Zongsoft.Resources;

namespace Zongsoft.Externals.Aliyun.Notification
{
	[CommandOption("Type", typeof(NotificationType), NotificationType.Message, "")]
	[CommandOption("DeviceType", typeof(NotificationDeviceType), NotificationDeviceType.Android, "")]
	[CommandOption("TargetType", typeof(NotificationTargetType), NotificationTargetType.Alias, "")]
	[CommandOption("Target", typeof(string), null, true, "")]
	public class NotificationSendCommand : CommandBase<CommandContext>
	{
		#region 成员字段
		private NotificationSender _sender;
		#endregion

		#region 公共属性
		public NotificationSender Sender
		{
			get
			{
				return _sender;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_sender = value;
			}
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(CommandContext context)
		{
			if(context.Expression.Arguments.Length == 0)
				throw new CommandException(ResourceUtility.GetString("Text.MissingCommandArguments"));

			var destination = context.Expression.Options.GetValue<string>("target");

			if(string.IsNullOrWhiteSpace(destination))
				throw new CommandOptionMissingException("target");

			var settings = new NotificationSenderSettings(context.Expression.Options.GetValue<NotificationType>("type"),
			                                              context.Expression.Options.GetValue<NotificationDeviceType>("deviceType"),
			                                              context.Expression.Options.GetValue<NotificationTargetType>("targetType"));

			var results = new List<NotificationResult>();

			foreach(var argument in context.Expression.Arguments)
			{
				var result = _sender.Send(argument, destination, settings);

				if(result != null)
				{
					if(!result.IsSucceed)
						context.Error.WriteLine(ResourceUtility.GetString("Text.NotificationSendCommand.Faild"));

					results.Add(result);
				}
			}

			if(results.Count == 0)
				return null;
			else if(results.Count == 1)
				return results[0];

			return results;
		}
		#endregion
	}
}

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

namespace Zongsoft.Externals.Aliyun.Pushing
{
	[CommandOption("Name", typeof(string), null, "Text.NotificationSendCommand.Options.Name")]
	[CommandOption("Title", typeof(string), null, "Text.NotificationSendCommand.Options.Title")]
	[CommandOption("Expiry", typeof(int), -1, "Text.NotificationSendCommand.Options.Expiry")]
	[CommandOption("Type", typeof(PushingType), PushingType.Message, "Text.NotificationSendCommand.Options.Type")]
	[CommandOption("DeviceType", typeof(PushingDeviceType), PushingDeviceType.Android, "Text.NotificationSendCommand.Options.DeviceType")]
	[CommandOption("TargetType", typeof(PushingTargetType), PushingTargetType.Alias, "Text.NotificationSendCommand.Options.TargetType")]
	[CommandOption("Target", typeof(string), null, true, "Text.NotificationSendCommand.Options.Target")]
	public class PushingSendCommand : CommandBase<CommandContext>
	{
		#region 成员字段
		private PushingSender _sender;
		#endregion

		#region 构造函数
		public PushingSendCommand() : base("Send")
		{
		}

		public PushingSendCommand(string name) : base(name)
		{
		}
		#endregion

		#region 公共属性
		public PushingSender Sender
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
			if(context.Parameter == null && context.Expression.Arguments.Length == 0)
				throw new CommandException(ResourceUtility.GetString("Text.MissingCommandArguments"));

			var destination = context.Expression.Options.GetValue<string>("target");

			if(string.IsNullOrWhiteSpace(destination))
				throw new CommandOptionMissingException("target");

			var settings = new PushingSenderSettings(context.Expression.Options.GetValue<PushingType>("type"),
														  context.Expression.Options.GetValue<PushingDeviceType>("deviceType"),
														  context.Expression.Options.GetValue<PushingTargetType>("targetType"),
														  context.Expression.Options.GetValue<int>("expiry"));

			var results = new List<ICommandResult>();

			if(context.Parameter != null)
			{
				var content = this.GetContent(context.Parameter);

				var result = this.Send(
					context.Expression.Options.GetValue<string>("name"),
					context.Expression.Options.GetValue<string>("title"),
					content, destination, settings, _ => context.Error.WriteLine(ResourceUtility.GetString("Text.NotificationSendCommand.Faild")));

				if(result != null)
				{
					results.Add(result.ToCommandResult());
				}
			}

			foreach(var argument in context.Expression.Arguments)
			{
				var result = this.Send(
					context.Expression.Options.GetValue<string>("name"),
					context.Expression.Options.GetValue<string>("title"),
					argument, destination, settings, _ => context.Error.WriteLine(ResourceUtility.GetString("Text.NotificationSendCommand.Faild")));

				if(result != null)
				{
					results.Add(result.ToCommandResult());
				}
			}

			if(results.Count == 0)
				return null;
			else if(results.Count == 1)
				return results[0];

			return results;
		}
		#endregion

		#region 私有方法
		private PushingResult Send(string name, string title, string content, string destination, PushingSenderSettings settings, Action<PushingResult> onFaild)
		{
			var result = Utility.ExecuteTask(() => _sender.SendAsync(name, title, content, destination, settings));

			if(result != null)
			{
				if(!result.IsSucceed && onFaild != null)
					onFaild(result);
			}

			return result;
		}

		private string GetContent(object value)
		{
			if(value == null)
				return null;

			if(value is string)
				return (string)value;

			if(value is System.Text.StringBuilder || Zongsoft.Common.TypeExtension.IsScalarType(value.GetType()))
				return value.ToString();

			return Zongsoft.Runtime.Serialization.Serializer.Json.Serialize(value);
		}
		#endregion
	}
}

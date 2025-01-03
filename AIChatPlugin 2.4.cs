using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Plugin
{
    [ApiVersion(2, 1)]
    public class AIChatPlugin : TerrariaPlugin  //AI聊天插件
    {
        #region 插件信息
        public override string Author => "镜奇路蓝";
        public override string Description => "AIChatPlugin ";
        public override string Name => "AIChatPlugin ";
        public override Version Version => new Version(2, 4);
        #endregion
        #region 插件启动
        public AIChatPlugin(Main game) : base(game)  //插件加载时执行的代码
        {
            base.Order = 1;  //插件加载顺序
            LoadConfig();  //加载配置
        }
        public override void Initialize()  //插件命令注册
        {
            Commands.ChatCommands.Add(new Command("tshock.cfg.reload", AIQCSXW, "aiclearall"));
            Commands.ChatCommands.Add(new Command("tshock.cfg.reload", AIreload, "reload"));
            Commands.ChatCommands.Add(new Command("tshock.canchat", ChatWithAICommand, "ab"));
            Commands.ChatCommands.Add(new Command("tshock.canchat", BotReset, "bcz"));
            Commands.ChatCommands.Add(new Command("tshock.canchat", BotHelp, "bbz"));
            PlayerHooks.PlayerLogout += OnPlayerLogout;  //注册玩家登出钩子
            ServerApi.Hooks.ServerChat.Register(this, OnChat);  //注册聊天钩子
        }
        #endregion
        #region 插件卸载
        protected override void Dispose(bool disposing)  //插件卸载时执行的代码
        {
            if (disposing)
            {
                PlayerHooks.PlayerLogout -= OnPlayerLogout;  //卸载玩家登出钩子
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat); //卸载聊天钩子
                Commands.ChatCommands.RemoveAll(cmd => cmd.CommandDelegate.Method?.DeclaringType?.Assembly == Assembly.GetExecutingAssembly());  //移除插件所有命令
                playerContexts.Clear();  //清理所有玩家的上下文记录
                isProcessing.Clear();  //清理所有玩家的处理状态
            }
        }
        #endregion
        #region 创建配置
        public static Configuration Config { get; private set; } = new Configuration();  //配置文件类
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "AI聊天配置.json");  //创建配置文件在tshock目录下
        public static string AIMSQH => Config.AIMSQH;  //AI模型选择
        public static string AILTCF => Config.AILTCF;  //AI聊天触发词
        public static int AIZSXZ => Config.AIZSXZ;  //AI字数限制
        public static int AIHH => Config.AIHH;  //AI自动换行字数
        public static int AISXW => Config.AISXW;  //AI上下文数量限制
        public static bool AILWSS => Config.AILWSS;  //启用联网搜索
        public static bool AIXXZF => Config.AIXXZF;  //转发QQ群总开关
        public static int QQQL => Config.QQQL;  //转发QQ群
        public static string AISET => Config.AISET;  //AI自定义设定
        public static double AIWD => Config.AIWD;  //temperature温度
        public static double AIHCY => Config.AIHCY;  //top_p核采样
        public class Configuration  //默认配置文件
        {
            [JsonProperty("模型选择：1为通用，2为速度")]
            public string AIMSQH { get; set; } = "2";
            [JsonProperty("聊天触发AI提示词")]
            public string AILTCF { get; set; } = "AI";
            [JsonProperty("AI回答字数限制")]
            public int AIZSXZ { get; set; } = 666;
            [JsonProperty("AI回答换行字数")]
            public int AIHH { get; set; } = 50;
            [JsonProperty("上下文限制")]
            public int AISXW { get; set; } = 10;
            [JsonProperty("启用联网搜索")]
            public bool AILWSS { get; set; } = true;
            [JsonProperty("转发QQ群总开关")]
            public bool AIXXZF { get; set; } = false;
            [JsonProperty("将控制台的消息转发到QQ群，实现群聊中CaiBot自带AI对话功能")]
            public int QQQL { get; set; } = 0;
            [JsonProperty("AI设定")]
            public string AISET { get; set; } = "你是一个简洁高效的AI，擅长用一句话精准概括复杂问题。";
            [JsonProperty("temperature温度")]
            public double AIWD { get; set; } = 0.5;
            [JsonProperty("top_p核采样")]
            public double AIHCY { get; set; } = 0.5;
        }
        #endregion
        #region 读取配置
        private void LoadConfig()  //创建配置和读取配置
        {
            if (!File.Exists(FilePath))  //配置文件不存在
            {
                Config = new Configuration();  //创建默认配置
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);  //序列化配置
                File.WriteAllText(FilePath, json);  //写入配置文件
            }
            else
            {
                try
                {
                    string jsonContent = File.ReadAllText(FilePath);  //读取配置文件内容
                    Configuration tempConfig = JsonConvert.DeserializeObject<Configuration>(jsonContent) ?? new Configuration();  //反序列化配置
                    if (tempConfig != null)  //配置不为空
                    {
                        if (tempConfig.AIMSQH != "1" && tempConfig.AIMSQH != "2")  //模型切换值无效
                        {
                            TShock.Log.ConsoleError($"[AI聊天插件] 模式无效，已保留原配置，并使用默认值 2 ");  //输出错误信息
                            tempConfig.AIMSQH = "2";  //使用默认值
                        }
                        //彻底解决配置文件相关的错误，并保留原配置
                        tempConfig.AILTCF = tempConfig.AILTCF ?? "AI";
                        tempConfig.AIZSXZ = tempConfig.AIZSXZ > 0 ? tempConfig.AIZSXZ : 666;
                        tempConfig.AIHH = tempConfig.AIHH > 0 ? tempConfig.AIHH : 50;
                        tempConfig.AISXW = tempConfig.AISXW > 0 ? tempConfig.AISXW : 10;
                        tempConfig.AILWSS = tempConfig.AILWSS; //布尔值不需要额外检查
                        tempConfig.AIXXZF = tempConfig.AIXXZF; //布尔值不需要额外检查
                        tempConfig.QQQL = tempConfig.QQQL >= 0 ? tempConfig.QQQL : 0;
                        tempConfig.AISET = tempConfig.AISET ?? "你是一个简洁高效的AI，擅长用一句话精准概括复杂问题。";
                        tempConfig.AIWD = tempConfig.AIWD >= 0 ? tempConfig.AIWD : 0.5;
                        tempConfig.AIHCY = tempConfig.AIHCY >= 0 ? tempConfig.AIHCY : 0.5;
                        Config = tempConfig;  //更新配置
                    }
                }
                catch (Exception ex)  //读取配置文件时发生错误
                {
                    TShock.Log.ConsoleError($"[AI聊天插件] 加载配置时发生错误，已保留原配置，使用默认值，错误信息：{ex.Message}");
                }
            }
        }
        private void AIreload(CommandArgs args)  //显示配置信息并重新加载配置
        {
            LoadConfig();  //重新加载配置
            args.Player.SendSuccessMessage("[AI聊天插件] 配置已重载");
            string configInfo = $"当前AI聊天配置：\n" +
                                $"模型：{Config.AIMSQH}\n" +
                                $"聊天触发词：{Config.AILTCF}\n" +
                                $"回答字限制：{Config.AIZSXZ}\n" +
                                $"回答换行字：{Config.AIHH}\n" +
                                $"上下文限制：{Config.AISXW}\n" +
                                $"联网搜索：{Config.AILWSS}\n" +
                                $"转发总开关：{Config.AIXXZF}\n" +
                                $"转发群：{Config.QQQL}\n" +
                                $"设定：{Config.AISET}\n" +
                                $"temperature温度：{Config.AIWD}\n" +
                                $"top_p核采样：{Config.AIHCY}";
            TShock.Log.ConsoleInfo(configInfo);
        }
        #endregion
        #region 帮助信息
        private void BotHelp(CommandArgs args)  //显示帮助信息
        {
            string helpMessage = "\n     [i:1344]AI聊天插件帮助信息[i:1344]\n" +
                                 "[i:1344]/ab                   - 向AI提问\n" +
                                 "[i:1344]/bzc                  - 清除您的上下文\n" +
                                 "[i:1344]/bbz                  - 显示此帮助信息\n" +
                                 "[i:1344]/aiclearall           - 清除所有人的上下文:\n" +
                                 "[i:1344]注意:\n" +
                                $"[i:1344] - 聊天栏最前面输入“{AILTCF}”会触发AI\n" +
                                 "[i:1344] - 请不问AI敏感问题，AI回答不了，不信你试试。\n";
            args.Player.SendInfoMessage(helpMessage);
        }
        #endregion
        #region 问题审核
        private void ChatWithAICommand(CommandArgs args)  //命令触发AI回答
        {
            if (args.Parameters.Count == 0)  //参数为空
            {
                args.Player.SendErrorMessage("[i:1344]请输入您想询问的内容！[i:1344]");
                return;  //返回
            }
            string question = string.Join(" ", args.Parameters);  //获取问题
            ChatWithAI(args.Player, question);  //调用AI聊天方法
        }
        private void OnChat(ServerChatEventArgs args)  //聊天开头输入AI 触发聊天事件
        {
            string message = args.Text;  //获取消息
            string triggerPhrase = $"{AILTCF}";  //获取触发词
            if (message.StartsWith(triggerPhrase))  //消息开头触发词
            {
                int triggerLength = triggerPhrase.Length;  //触发词长度
                string userQuestion = message.Substring(triggerLength).Trim();  //获取用户问题
                if (!string.IsNullOrWhiteSpace(userQuestion))  //问题不为空
                {
                    TSPlayer player = TShock.Players[args.Who];  //获取玩家对象
                    if (player != null)  //玩家对象不为空
                    {
                        ChatWithAI(player, userQuestion);  //调用AI聊天方法
                    }
                }
            }
        }
        private static Dictionary<int, List<string>> playerContexts = new Dictionary<int, List<string>>();  //玩家上下文记录
        private static readonly Dictionary<int, bool> isProcessing = new Dictionary<int, bool>();  //玩家是否正在处理
        private static DateTime lastCmdTime = DateTime.MinValue;  //上一次命令时间
        private static readonly int cooldownDuration = 6;  //命令冷却时间
        private void ChatWithAI(TSPlayer player, string question)  //聊天触发AI回答
        {
            int playerIndex = player.Index;  //获取玩家索引
            if (isProcessing.ContainsKey(playerIndex) && isProcessing[playerIndex])  //玩家正在处理请求
            {
                player.SendErrorMessage("[i:1344]有其他玩家在询问问题，请排队[i:1344]");
                return;  //返回
            }
            if ((DateTime.Now - lastCmdTime).TotalSeconds < cooldownDuration)  //命令冷却中
            {
                int remainingTime = cooldownDuration - (int)(DateTime.Now - lastCmdTime).TotalSeconds;  //计算剩余冷却时间
                player.SendErrorMessage($"[i:1344]请耐心等待 {remainingTime} 秒后再输入![i:1344]");
                return;  //返回
            }
            if (string.IsNullOrWhiteSpace(question))  //问题为空
            {
                player.SendErrorMessage("[i:1344]您的问题不能为空，请输入您想询问的内容！[i:1344]");
                return;  //返回
            }
            lastCmdTime = DateTime.Now;  //更新上一次命令时间
            player.SendSuccessMessage("[i:1344]正在处理您的请求，请稍候... [i:1344]");
            isProcessing[playerIndex] = true;  //玩家开始处理请求
            AddToContext(playerIndex, question);  //记录玩家的上下文
            Task.Run(async () =>   //异步处理请求
            {
                try
                {
                    await ProcessAIChat(player, question);  //处理AI聊天请求
                }
                catch (Exception ex)  //处理请求时发生错误1
                {
                    string AICLQQSFSCW1 = $"[AI聊天插件] 处理请求时发生错误！请检查一下问题是否格式正确，详细信息：{ex.Message}\n";
                    TShock.Log.ConsoleError(AICLQQSFSCW1);
                    if (player.IsLoggedIn)  //玩家在线才发送错误信息
                    {
                        player.SendErrorMessage(AICLQQSFSCW1);
                    }
                }
                finally
                {
                    isProcessing[playerIndex] = false;  //请求处理完成
                }
            });
        }
        #endregion
        #region 请求处理
        private class AIResponse  //AI返回结果
        {
            public Choice[] choices { get; set; } = Array.Empty<Choice>();  //选项列表
        }
        private class Choice  //AI返回结果中的选项
        {
            public Message message { get; set; } = new Message();  //消息
        }
        private class Message  //AI返回结果中的消息
        {
            public string content { get; set; } = string.Empty;  //内容
        }
        private async Task ProcessAIChat(TSPlayer player, string question)  //向AI发送请求并处理返回结果
        {
            try
            {
                string cleanedQuestion = CleanMessage(question);  //清理消息中的特殊字符
                List<string> context = GetContext(player.Index);  //获取上下文记录
                string formattedContext = context.Count > 0  //历史对话大于0才携带历史对话给AI
                    ? "以下是之前的对话记录，请参考这些内容来回答当前问题（注意关键词）：" + string.Join("\n", context) + "\n"
                    : "";
                string model = Config.AIMSQH == "1" ? "glm-4-flash" : "GLM-4V-Flash";  //选择模型
                using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };  //超时时间为100秒
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer 742701d3fea4bed898578856989cb03c.5mKVzv5shSIqkkS7");  //设置API认证信息
                var tools = new List<object>();  //工具列表
                if (Config.AILWSS)  //根据配置是否启动联网搜索功能
                {
                    tools.Add(new
                    {
                        type = "web_search",
                        web_search = new
                        {
                            enable = true,
                            search_query = question
                        }
                    });
                };
                var requestBody = new
                {
                    model = model,  //模型选择
                    messages = new[]
                    {
                        new { role = "user", content = formattedContext + $"（设定：{AISET}）问题:\n那，" + question }  //问题请求
                    },
                    tools = tools,
                    temperature = AIWD,  //temperature温度
                    top_p = AIHCY,  //top_p核采样
                };
                var response = await client.PostAsync("https://open.bigmodel.cn/api/paas/v4/chat/completions",  //请求API地址
                    new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));  //向AI发送请求
                if (response.IsSuccessStatusCode)  //请求成功
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();  //获取API返回结果
                    var result = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);  //反序列化AI返回结果
                    if (result?.choices != null && result.choices.Length > 0)  //AI返回有效结果
                    {
                        var firstChoice = result.choices[0];  //获取第一个AI回答
                        string responseMessage = firstChoice.message.content;  //AI回答
                        responseMessage = CleanMessage(responseMessage);  //清理消息中的特殊字符
                        if (responseMessage.Length > AIZSXZ)  //消息长度超过限制
                        {
                            responseMessage = TruncateMessage(responseMessage);  //截断消息
                        }
                        if (!player.IsLoggedIn)  //玩家是离线才调用转发消息方法
                        {
                            _ = ForwardMessageToCaiBot(player.Name, responseMessage);  //调用CaiBot方法转发消息到群聊
                        }
                        string formattedQuestion = FormatMessage(question), formattedResponse = FormatMessage(responseMessage);  //格式化消息
                        string broadcastMessage = $"\n用户 | {player.Name} | 提问: \n{formattedQuestion}\nAI回复: \n{formattedResponse}\n";  //发送格式化后的消息
                        TSPlayer.All.SendInfoMessage(broadcastMessage); TShock.Log.ConsoleInfo(broadcastMessage);  //广播消息到所有玩家并且输出到控制台并记录日志
                        AddToContext(player.Index, responseMessage);  //记录上下文
                    }
                    else  //AI未返回有效结果
                    {
                        string AIWFHYXJG = "[AI聊天插件] 很抱歉，这次未获得有效的AI响应。\n";
                        TShock.Log.ConsoleError(AIWFHYXJG);
                        if (player.IsLoggedIn)
                        {
                            player.SendErrorMessage(AIWFHYXJG);
                        }
                    }
                }
                else  //请求失败
                {
                    string AIQQSB = $"[AI聊天插件] AI未能及时响应，请稍后重试，状态码：{response.StatusCode}\n可以先尝试输入 /bcz 来清理上下文记录，如果问题不能解决，请联系此插件作者：镜奇路蓝";
                    TShock.Log.ConsoleError(AIQQSB);
                    if (player.IsLoggedIn)
                    {
                        player.SendErrorMessage(AIQQSB);
                    }
                }
            }
            catch (TaskCanceledException)  //请求超时
            {
                string AIQQCS = "[AI聊天插件] 请求超时！请检查网络连接和API状态，确保一切正常。\n";
                TShock.Log.ConsoleError(AIQQCS);
                if (player.IsLoggedIn)
                {
                    player.SendErrorMessage(AIQQCS);
                }
            }
            catch (Exception ex)  //其他错误
            {
                string AIQTCW = $"[AI聊天插件] 出现错误！请检查相关设置与请求！详细信息：{ex.Message}\n";
                TShock.Log.ConsoleError(AIQTCW);
                if (player.IsLoggedIn)
                {
                    player.SendErrorMessage(AIQTCW);
                }
            }
        }
        #endregion
        #region 上下文限制
        private void BotReset(CommandArgs args)  //上下文重置
        {
            if (playerContexts.ContainsKey(args.Player.Index))  //上下文记录存在
            {
                playerContexts.Remove(args.Player.Index);  //移除上下文记录
                args.Player.SendSuccessMessage("[i:1344]您的上下文记录已重置！[i:1344]");
            }
            else  //上下文记录不存在
            {
                args.Player.SendErrorMessage("[i:1344]您当前没有上下文记录！[i:1344]");
            }
        }
        private List<string> GetContext(int playerId)  //获取上下文记录
        {
            if (playerContexts.ContainsKey(playerId))  //上下文记录存在
            {
                return playerContexts[playerId];  //返回上下文记录
            }
            else
            {
                return new List<string>();  //返回空列表
            }
        }
        private void AddToContext(int playerId, string message)  //上下文记录和清理
        {
            if (!playerContexts.ContainsKey(playerId))  //上下文记录不存在
            {
                playerContexts[playerId] = new List<string>();  //创建上下文记录
            }
            if (playerContexts[playerId].Count >= AISXW)  //上下文记录超过限制
            {
                playerContexts[playerId].RemoveAt(0);  //移除最早的消息
            }
            playerContexts[playerId].Add(message);  //添加新的消息
        }
        private void OnPlayerLogout(PlayerLogoutEventArgs e)  //玩家退出时清除上下文信息
        {
            int playerId = e.Player.Index;
            if (playerContexts.ContainsKey(playerId))  //上下文记录存在
            {
                playerContexts.Remove(playerId);  //移除上下文记录
            }
            if (isProcessing.ContainsKey(playerId))  //玩家正在处理请求
            {
                isProcessing.Remove(playerId);  //移除玩家正在处理请求
            }
        }
        public void AIQCSXW(CommandArgs args)  //清除所有玩家的上下文
        {
            if (playerContexts.Count == 0)  //上下文记录为空
            {
                args.Player.SendInfoMessage("[AI聊天插件] 当前没有任何人的上下文记录。");
            }
            else
            {
                playerContexts.Clear();  //清除上下文记录
                args.Player.SendSuccessMessage("[AI聊天插件] 所有人的上下文已清除。");
            }
        }
        #endregion
        #region 回答限制/优化
        private string CleanMessage(string message)  //清理消息中的特殊字符
        {
            if (Regex.IsMatch(message, @"[\uD800-\uDBFF][\uDC00-\uDFFF]"))  //处理emoji
            {
                return string.Empty;  //过滤emoji
            }
            return message;  //返回消息
        }
        private string FormatMessage(string message)  //AI回答自动换行
        {
            StringBuilder formattedMessage = new StringBuilder();  //格式化消息
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(message);  //获取文本元素枚举器
            int currentLength = 0;   //当前已输出的字符长度
            while (enumerator.MoveNext())  //遍历文本元素
            {
                string textElement = enumerator.GetTextElement();  //获取当前文本元素
                if (currentLength + textElement.Length > AIHH)  //超出换行字数限制
                {
                    if (formattedMessage.Length > 0)  //当前已有换行
                    {
                        formattedMessage.AppendLine();  //添加换行符
                    }
                    currentLength = 0;  //重置已输出的字符长度
                }
                formattedMessage.Append(textElement);  //添加当前文本元素
                currentLength += textElement.Length;  //更新已输出的字符长度
            }
            return formattedMessage.ToString();  //返回格式化后的消息
        }
        private string TruncateMessage(string message)  //AI回答字数限制
        {
            if (message.Length <= AIZSXZ) return message;   //消息长度不超过限制
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(message);  //获取文本元素枚举器
            StringBuilder truncated = new StringBuilder();  //截断消息
            int count = 0;   //已输出的字符数量
            while (enumerator.MoveNext())  //遍历文本元素
            {
                string textElement = enumerator.GetTextElement();  //获取当前文本元素
                if (truncated.Length + textElement.Length > AIZSXZ) break;  //超出字数限制
                truncated.Append(textElement);  //添加当前文本元素
                count++;  //更新已输出的字符数量
            }
            if (count == 0 || truncated.Length >= AIZSXZ)  //消息长度不足或已超出字数限制
            {
                truncated.Append("\n\n\n[i:1344]超出字数限制" + AIZSXZ + "已省略！[i:1344]");
            }
            return truncated.ToString();  //返回截断后的消息
        }
        #endregion
        #region CaiBot转发到群聊
        private object? caiBotPlugin;  //CaiBot插件对象
        private Task ForwardMessageToCaiBot(string playerName, string message)  //CaiBot转发消息到群聊
        {
            // 检查是否开启转发QQ群功能
            if (!Config.AIXXZF)  // 如果AIXXZF为false，直接返回
            {
                return Task.CompletedTask;
            }
            if (caiBotPlugin == null)  //CaiBot插件不存在
            {
                return Task.CompletedTask;  //直接返回
            }
            try
            {
                var messageHandleType = caiBotPlugin.GetType().Assembly.GetType("CaiBot.MessageHandle");  //获取 CaiBot 插件的 MessageHandle 类型
                if (messageHandleType == null)  //CaiBot 插件的 MessageHandle 类型不存在
                {
                    TShock.Log.ConsoleError("[AI聊天插件] 未找到 CaiBot 插件的 MessageHandle 类型。");
                    return Task.CompletedTask;  //直接返回
                }
                var sendDateAsyncMethod = messageHandleType.GetMethod("SendDateAsync", new[] { typeof(string) });  //获取 CaiBot 插件的 SendDateAsync 方法
                if (sendDateAsyncMethod == null)  //CaiBot 插件的 SendDateAsync 方法不存在
                {
                    TShock.Log.ConsoleError("[AI聊天插件] 未找到 CaiBot 插件的 SendDateAsync 方法。");
                    return Task.CompletedTask;  //直接返回
                }
                //检查群号是否有效
                if (Config.QQQL >= 10000 && Config.QQQL <= 999999999)  //群号范围是6到9位数，且从10000开始
                {
                    var result = new  //创建 CaiBot 插件的 MessageHandle 类型实例
                    {
                        type = "chat",
                        chat = $"[AI聊天转发]\n{message}",
                        group = Config.QQQL,
                    };
                    string jsonMessage = JsonConvert.SerializeObject(result);  //序列化 CaiBot 插件的 MessageHandle 类型实例
                    var invokeResult = sendDateAsyncMethod.Invoke(null, new object[] { jsonMessage });  //调用 CaiBot 插件的 SendDateAsync 方法
                    if (invokeResult == null)  //调用 CaiBot 插件的 SendDateAsync 方法返回了 null
                    {
                        TShock.Log.ConsoleError("[AI聊天插件] 调用 SendDateAsync 方法时返回了 null。");
                        return Task.CompletedTask;  //直接返回
                    }
                }
                else  //群号无效
                {
                    return Task.CompletedTask;  //直接返回
                }
            }
            catch (Exception ex)  //转发消息到 CaiBot 插件时发生错误
            {
                TShock.Log.ConsoleError($"[AI聊天插件] 转发消息到 CaiBot 插件时发生错误: {ex.Message}");
            }
            return Task.CompletedTask;  //直接返回
        }
        #endregion
    }
}

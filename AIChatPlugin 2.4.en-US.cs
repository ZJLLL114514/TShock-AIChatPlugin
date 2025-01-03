//The following is the code for converting Chinese to English, if you have any questions, please feedback
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
    public class AIChatPlugin : TerrariaPlugin  // AI Chat Plugin
    {
        #region Plugin Information
        public override string Author => "Mirror Qiluban";
        public override string Description => "AIChatPlugin ";
        public override string Name => "AIChatPlugin ";
        public override Version Version => new Version(2, 4);
        #endregion
        #region Plugin Startup
        public AIChatPlugin(Main game) : base(game)  // Code executed when the plugin is loaded
        {
            base.Order = 1;  // Plugin loading order
            LoadConfig();  // Load the configuration
        }
        public override void Initialize()  // Plugin command registration
        {
            Commands.ChatCommands.Add(new Command("tshock.cfg.reload", AIQCSXW, "aiclearall"));
            Commands.ChatCommands.Add(new Command("tshock.cfg.reload", AIreload, "reload"));
            Commands.ChatCommands.Add(new Command("tshock.canchat", ChatWithAICommand, "ab"));
            Commands.ChatCommands.Add(new Command("tshock.canchat", BotReset, "bcz"));
            Commands.ChatCommands.Add(new Command("tshock.canchat", BotHelp, "bbz"));
            PlayerHooks.PlayerLogout += OnPlayerLogout;  // Register player logout hook
            ServerApi.Hooks.ServerChat.Register(this, OnChat);  // Register chat hook
        }
        #endregion
        #region Plugin Unloading
        protected override void Dispose(bool disposing)  // Code executed when the plugin is unloaded
        {
            if (disposing)
            {
                PlayerHooks.PlayerLogout -= OnPlayerLogout;  // Unregister player logout hook
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat); // Unregister chat hook
                Commands.ChatCommands.RemoveAll(cmd => cmd.CommandDelegate.Method?.DeclaringType?.Assembly == Assembly.GetExecutingAssembly());  // Remove all commands from the plugin
                playerContexts.Clear();  // Clear all player context records
                isProcessing.Clear();  // Clear all player processing states
            }
        }
        #endregion
        #region Create Configuration
        public static Configuration Config { get; private set; } = new Configuration();  // Configuration file class
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "AIChatConfig.json");  // Create configuration file in tshock directory
        public static string AIMSQH => Config.AIMSQH;  // AI model selection
        public static string AILTCF => Config.AILTCF;  // AI chat trigger words
        public static int AIZSXZ => Config.AIZSXZ;  // AI word limit
        public static int AIHH => Config.AIHH;  // AI automatic line break count
        public static int AISXW => Config.AISXW;  // AI context limit
        public static bool AILWSS => Config.AILWSS;  // Enable online search
        public static bool AIXXZF => Config.AIXXZF;  // Forward QQ group switch
        public static int QQQL => Config.QQQL;  // Forward QQ group
        public static string AISET => Config.AISET;  // AI custom settings
        public static double AIWD => Config.AIWD;  // temperature
        public static double AIHCY => Config.AIHCY;  // top_p sampling
        public class Configuration  // Default configuration file
        {
            [JsonProperty("Model selection: 1 for general, 2 for speed")]
            public string AIMSQH { get; set; } = "2";
            [JsonProperty("Chat trigger AI prompt words")]
            public string AILTCF { get; set; } = "AI";
            [JsonProperty("AI response word limit")]
            public int AIZSXZ { get; set; } = 666;
            [JsonProperty("AI response line break word count")]
            public int AIHH { get; set; } = 50;
            [JsonProperty("Context limit")]
            public int AISXW { get; set; } = 10;
            [JsonProperty("Enable online search")]
            public bool AILWSS { get; set; } = true;
            [JsonProperty("Forward QQ group switch")]
            public bool AIXXZF { get; set; } = false;
            [JsonProperty("Forward console messages to QQ group for CaiBot's built-in AI chat function")]
            public int QQQL { get; set; } = 0;
            [JsonProperty("AI settings")]
            public string AISET { get; set; } = "You are a concise and efficient AI, good at summarizing complex issues in one sentence.";
            [JsonProperty("Temperature")]
            public double AIWD { get; set; } = 0.5;
            [JsonProperty("Top_p sampling")]
            public double AIHCY { get; set; } = 0.5;
        }
        #endregion
        #region Read Configuration
        private void LoadConfig()  // Create and read configuration
        {
            if (!File.Exists(FilePath))  // Configuration file does not exist
            {
                Config = new Configuration();  // Create default configuration
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);  // Serialize configuration
                File.WriteAllText(FilePath, json);  // Write to configuration file
            }
            else
            {
                try
                {
                    string jsonContent = File.ReadAllText(FilePath);  // Read configuration file content
                    Configuration tempConfig = JsonConvert.DeserializeObject<Configuration>(jsonContent) ?? new Configuration();  // Deserialize configuration
                    if (tempConfig != null)  // Configuration is not empty
                    {
                        if (tempConfig.AIMSQH != "1" && tempConfig.AIMSQH != "2")  // Invalid model switch value
                        {
                            TShock.Log.ConsoleError($"[AIChatPlugin] Invalid mode, original configuration retained, using default value 2 ");  // Output error message
                            tempConfig.AIMSQH = "2";  // Use default value
                        }
                        // Thoroughly solve configuration file-related errors and retain original configuration
                        tempConfig.AILTCF = tempConfig.AILTCF ?? "AI";
                        tempConfig.AIZSXZ = tempConfig.AIZSXZ > 0 ? tempConfig.AIZSXZ : 666;
                        tempConfig.AIHH = tempConfig.AIHH > 0 ? tempConfig.AIHH : 50;
                        tempConfig.AISXW = tempConfig.AISXW > 0 ? tempConfig.AISXW : 10;
                        tempConfig.AILWSS = tempConfig.AILWSS; // Boolean value does not need extra checks
                        tempConfig.AIXXZF = tempConfig.AIXXZF; // Boolean value does not need extra checks
                        tempConfig.QQQL = tempConfig.QQQL >= 0 ? tempConfig.QQQL : 0;
                        tempConfig.AISET = tempConfig.AISET ?? "You are a concise and efficient AI, good at summarizing complex issues in one sentence.";
                        tempConfig.AIWD = tempConfig.AIWD >= 0 ? tempConfig.AIWD : 0.5;
                        tempConfig.AIHCY = tempConfig.AIHCY >= 0 ? tempConfig.AIHCY : 0.5;
                        Config = tempConfig;  // Update configuration
                    }
                }
                catch (Exception ex)  // Error occurred while reading configuration file
                {
                    TShock.Log.ConsoleError($"[AIChatPlugin] Error occurred while loading configuration, original configuration retained, using default values, error message: {ex.Message}");
                }
            }
        }
        private void AIreload(CommandArgs args)  // Display configuration info and reload configuration
        {
            LoadConfig();  // Reload configuration
            args.Player.SendSuccessMessage("[AIChatPlugin] Configuration has been reloaded");
            string configInfo = $"Current AI chat configuration：\n" +
                                $"Model：{Config.AIMSQH}\n" +
                                $"Chat trigger words：{Config.AILTCF}\n" +
                                $"Response word limit：{Config.AIZSXZ}\n" +
                                $"Response line break count：{Config.AIHH}\n" +
                                $"Context limit：{Config.AISXW}\n" +
                                $"Online search：{Config.AILWSS}\n" +
                                $"Forward switch：{Config.AIXXZF}\n" +
                                $"Forward group：{Config.QQQL}\n" +
                                $"Setting：{Config.AISET}\n" +
                                $"Temperature：{Config.AIWD}\n" +
                                $"Top_p sampling：{Config.AIHCY}";
            TShock.Log.ConsoleInfo(configInfo);
        }
        #endregion
        #region Help Information
        private void BotHelp(CommandArgs args)  // Display help information
        {
            string helpMessage = "\n     [i:1344]AI Chat Plugin Help Information[i:1344]\n" +
                                 "[i:1344]/ab                   - Ask AI\n" +
                                 "[i:1344]/bzc                  - Clear your context\n" +
                                 "[i:1344]/bbz                  - Display this help information\n" +
                                 "[i:1344]/aiclearall           - Clear everyone's context:\n" +
                                 "[i:1344]Note:\n" +
                                 $"[i:1344] - Type \"{AILTCF}\" at the start of the chat box to trigger AI\n" +
                                 "[i:1344] - Do not ask AI sensitive questions, it cannot answer them; you can try it and see.\n";
            args.Player.SendInfoMessage(helpMessage);
        }
        #endregion
        #region Question Review
        private void ChatWithAICommand(CommandArgs args)  // Command to trigger AI response
        {
            if (args.Parameters.Count == 0)  // Parameters are empty
            {
                args.Player.SendErrorMessage("[i:1344] Please enter the content you want to ask! [i:1344]");
                return;  // Return
            }
            string question = string.Join(" ", args.Parameters);  // Get the question
            ChatWithAI(args.Player, question);  // Call AI chat method
        }
        private void OnChat(ServerChatEventArgs args)  // Chat starts with AI triggering chat event
        {
            string message = args.Text;  // Get message
            string triggerPhrase = $"{AILTCF}";  // Get trigger words
            if (message.StartsWith(triggerPhrase))  // Message starts with trigger words
            {
                int triggerLength = triggerPhrase.Length;  // Trigger words length
                string userQuestion = message.Substring(triggerLength).Trim();  // Get user question
                if (!string.IsNullOrWhiteSpace(userQuestion))  // Question is not empty
                {
                    TSPlayer player = TShock.Players[args.Who];  // Get player object
                    if (player != null)  // Player object is not null
                    {
                        ChatWithAI(player, userQuestion);  // Call AI chat method
                    }
                }
            }
        }
        private static Dictionary<int, List<string>> playerContexts = new Dictionary<int, List<string>>();  // Player context records
        private static readonly Dictionary<int, bool> isProcessing = new Dictionary<int, bool>();  // Whether player is processing
        private static DateTime lastCmdTime = DateTime.MinValue;  // Last command time
        private static readonly int cooldownDuration = 6;  // Command cooldown time
        private void ChatWithAI(TSPlayer player, string question)  // Trigger AI response
        {
            int playerIndex = player.Index;  // Get player index
            if (isProcessing.ContainsKey(playerIndex) && isProcessing[playerIndex])  // Player is processing request
            {
                player.SendErrorMessage("[i:1344] Another player is asking a question, please wait in line [i:1344]");
                return;  // Return
            }
            if ((DateTime.Now - lastCmdTime).TotalSeconds < cooldownDuration)  // Command is in cooldown
            {
                int remainingTime = cooldownDuration - (int)(DateTime.Now - lastCmdTime).TotalSeconds;  // Calculate remaining cooldown time
                player.SendErrorMessage($"[i:1344] Please wait {remainingTime} seconds before entering again! [i:1344]");
                return;  // Return
            }
            if (string.IsNullOrWhiteSpace(question))  // Question is empty
            {
                player.SendErrorMessage("[i:1344] Your question cannot be empty, please enter your content! [i:1344]");
                return;  // Return
            }
            lastCmdTime = DateTime.Now;  // Update last command time
            player.SendSuccessMessage("[i:1344] Processing your request, please wait... [i:1344]");
            isProcessing[playerIndex] = true;  // Player is processing request
            AddToContext(playerIndex, question);  // Record player's context
            Task.Run(async () =>   // Asynchronously handle request
            {
                try
                {
                    await ProcessAIChat(player, question);  // Process AI chat request
                }
                catch (Exception ex)  // Error occurred while processing request
                {
                    string errorMessage = $"[AIChatPlugin] An error occurred while processing the request! Please check if the question format is correct, detailed information: {ex.Message}\n";
                    TShock.Log.ConsoleError(errorMessage);
                    if (player.IsLoggedIn)  // Send error message only if player is online
                    {
                        player.SendErrorMessage(errorMessage);
                    }
                }
                finally
                {
                    isProcessing[playerIndex] = false;  // Request processing complete
                }
            });
        }
        #endregion
        #region Request Processing
        private class AIResponse  // AI response result
        {
            public Choice[] choices { get; set; } = Array.Empty<Choice>();  // Choices list
        }
        private class Choice  // Options in AI response result
        {
            public Message message { get; set; } = new Message();  // Message
        }
        private class Message  // Message in AI response result
        {
            public string content { get; set; } = string.Empty;  // Content
        }
        private async Task ProcessAIChat(TSPlayer player, string question)  // Send request to AI and process response
        {
            try
            {
                string cleanedQuestion = CleanMessage(question);  // Clean special characters in message
                List<string> context = GetContext(player.Index);  // Get context records
                string formattedContext = context.Count > 0  // Only carry historical dialogue if greater than 0 for AI
                    ? "Here are the previous dialogue records, please refer to these to answer the current question (focus on keywords): " + string.Join("\n", context) + "\n"
                    : "";
                string model = Config.AIMSQH == "1" ? "glm-4-flash" : "GLM-4V-Flash";  // Choose model
                using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };  // Set timeout to 100 seconds
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer 742701d3fea4bed898578856989cb03c.5mKVzv5shSIqkkS7");  // Set API authorization information
                var tools = new List<object>();  // Tools list
                if (Config.AILWSS)  // Based on configuration whether to enable online search function
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
                    model = model,  // Model selection
                    messages = new[]
                    {
                        new { role = "user", content = formattedContext + $"(Setting: {AISET}) Question:\nWell, " + question }  // Question request
                    },
                    tools = tools,
                    temperature = AIWD,  // Temperature
                    top_p = AIHCY,  // Top_p sampling
                };
                var response = await client.PostAsync("https://open.bigmodel.cn/api/paas/v4/chat/completions",  // Request API address
                    new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));  // Send request to AI
                if (response.IsSuccessStatusCode)  // Request succeeded
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();  // Get API return result
                    var result = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);  // Deserialize AI return result
                    if (result?.choices != null && result.choices.Length > 0)  // AI returns valid result
                    {
                        var firstChoice = result.choices[0];  // Get first AI response
                        string responseMessage = firstChoice.message.content;  // AI response
                        responseMessage = CleanMessage(responseMessage);  // Clean special characters in message
                        if (responseMessage.Length > AIZSXZ)  // Message length exceeds limit
                        {
                            responseMessage = TruncateMessage(responseMessage);  // Truncate message
                        }
                        if (!player.IsLoggedIn)  // Player is offline, call the method to forward the message
                        {
                            _ = ForwardMessageToCaiBot(player.Name, responseMessage);  // Call CaiBot to forward message to group chat
                        }
                        string formattedQuestion = FormatMessage(question), formattedResponse = FormatMessage(responseMessage);  // Format message
                        string broadcastMessage = $"\nUser | {player.Name} | Question: \n{formattedQuestion}\nAI response: \n{formattedResponse}\n";  // Send formatted message
                        TSPlayer.All.SendInfoMessage(broadcastMessage); TShock.Log.ConsoleInfo(broadcastMessage);  // Broadcast message to all players and log to console
                        AddToContext(player.Index, responseMessage);  // Record context
                    }
                    else  // AI did not return a valid result
                    {
                        string noResponseMsg = "[AIChatPlugin] Sorry, no valid AI response this time.\n";
                        TShock.Log.ConsoleError(noResponseMsg);
                        if (player.IsLoggedIn)
                        {
                            player.SendErrorMessage(noResponseMsg);
                        }
                    }
                }
                else  // Request failed
                {
                    string requestFailedMsg = $"[AIChatPlugin] AI was unable to respond in time, please try again later, status code: {response.StatusCode}\n You may try typing /bcz to clear context records first, if the problem persists, please contact the plugin author: Mirror Qiluban";
                    TShock.Log.ConsoleError(requestFailedMsg);
                    if (player.IsLoggedIn)
                    {
                        player.SendErrorMessage(requestFailedMsg);
                    }
                }
            }
            catch (TaskCanceledException)  // Request timeout
            {
                string timeoutMsg = "[AIChatPlugin] The request timed out! Please check the network connection and API status to ensure everything is normal.\n";
                TShock.Log.ConsoleError(timeoutMsg);
                if (player.IsLoggedIn)
                {
                    player.SendErrorMessage(timeoutMsg);
                }
            }
            catch (Exception ex)  // Other errors
            {
                string errorMsg = $"[AIChatPlugin] An error occurred! Please check the relevant settings and requests! Detailed information: {ex.Message}\n";
                TShock.Log.ConsoleError(errorMsg);
                if (player.IsLoggedIn)
                {
                    player.SendErrorMessage(errorMsg);
                }
            }
        }
        #endregion
        #region Context Limiting
        private void BotReset(CommandArgs args)  // Context reset
        {
            if (playerContexts.ContainsKey(args.Player.Index))  // Context record exists
            {
                playerContexts.Remove(args.Player.Index);  // Remove context record
                args.Player.SendSuccessMessage("[i:1344] Your context record has been reset! [i:1344]");
            }
            else  // Context record does not exist
            {
                args.Player.SendErrorMessage("[i:1344] You currently have no context record! [i:1344]");
            }
        }
        private List<string> GetContext(int playerId)  // Get context record
        {
            if (playerContexts.ContainsKey(playerId))  // Context record exists
            {
                return playerContexts[playerId];  // Return context record
            }
            else
            {
                return new List<string>();  // Return empty list
            }
        }
        private void AddToContext(int playerId, string message)  // Context record maintenance
        {
            if (!playerContexts.ContainsKey(playerId))  // Context record does not exist
            {
                playerContexts[playerId] = new List<string>();  // Create context record
            }
            if (playerContexts[playerId].Count >= AISXW)  // Context record exceeds limit
            {
                playerContexts[playerId].RemoveAt(0);  // Remove the oldest message
            }
            playerContexts[playerId].Add(message);  // Add new message
        }
        private void OnPlayerLogout(PlayerLogoutEventArgs e)  // Clear context information when player logs out
        {
            int playerId = e.Player.Index;
            if (playerContexts.ContainsKey(playerId))  // Context record exists
            {
                playerContexts.Remove(playerId);  // Remove context record
            }
            if (isProcessing.ContainsKey(playerId))  // Player is processing request
            {
                isProcessing.Remove(playerId);  // Remove player's processing request
            }
        }
        public void AIQCSXW(CommandArgs args)  // Clear all players' contexts
        {
            if (playerContexts.Count == 0)  // Context record is empty
            {
                args.Player.SendInfoMessage("[AIChatPlugin] There are currently no context records for anyone.");
            }
            else
            {
                playerContexts.Clear();  // Clear context records
                args.Player.SendSuccessMessage("[AIChatPlugin] All contexts have been cleared.");
            }
        }
        #endregion
        #region Response Limits/Optimization
        private string CleanMessage(string message)  // Clean special characters in message
        {
            if (Regex.IsMatch(message, @"[\uD800-\uDBFF][\uDC00-\uDFFF]"))  // Handle emoji
            {
                return string.Empty;  // Filter emoji
            }
            return message;  // Return message
        }
        private string FormatMessage(string message)  // Auto line break in AI response
        {
            StringBuilder formattedMessage = new StringBuilder();  // Format message
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(message);  // Get text element enumerator
            int currentLength = 0;   // Current output character length
            while (enumerator.MoveNext())  // Traverse text elements
            {
                string textElement = enumerator.GetTextElement();  // Get current text element
                if (currentLength + textElement.Length > AIHH)  // Exceeds line break word limit
                {
                    if (formattedMessage.Length > 0)  // Current has line break
                    {
                        formattedMessage.AppendLine();  // Add line break
                    }
                    currentLength = 0;  // Reset current output character length
                }
                formattedMessage.Append(textElement);  // Add current text element
                currentLength += textElement.Length;  // Update current output character length
            }
            return formattedMessage.ToString();  // Return formatted message
        }
        private string TruncateMessage(string message)  // AI response word limit
        {
            if (message.Length <= AIZSXZ) return message;   // Message length does not exceed limit
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(message);  // Get text element enumerator
            StringBuilder truncated = new StringBuilder();  // Truncate message
            int count = 0;   // Number of characters output
            while (enumerator.MoveNext())  // Traverse text elements
            {
                string textElement = enumerator.GetTextElement();  // Get current text element
                if (truncated.Length + textElement.Length > AIZSXZ) break;  // Exceeds word limit
                truncated.Append(textElement);  // Add current text element
                count++;  // Update output character number
            }
            if (count == 0 || truncated.Length >= AIZSXZ)  // Message length insufficient or has exceeded word limit
            {
                truncated.Append("\n\n\n[i:1344] Exceeded word limit " + AIZSXZ + ", omitted! [i:1344]");
            }
            return truncated.ToString();  // Return truncated message
        }
        #endregion
        #region CaiBot Forward to Group Chat
        //The following code requires the Chinese software QQ to use
        private object? caiBotPlugin;  // CaiBot plugin object
        private Task ForwardMessageToCaiBot(string playerName, string message)  // CaiBot forward message to group chat
        {
            // Check if forwarding QQ group function is enabled
            if (!Config.AIXXZF)  // If AIXXZF is false, return directly
            {
                return Task.CompletedTask;
            }
            if (caiBotPlugin == null)  // CaiBot plugin does not exist
            {
                return Task.CompletedTask;  // Return directly
            }
            try
            {
                var messageHandleType = caiBotPlugin.GetType().Assembly.GetType("CaiBot.MessageHandle");  // Get CaiBot plugin MessageHandle type
                if (messageHandleType == null)  // CaiBot plugin MessageHandle type does not exist
                {
                    TShock.Log.ConsoleError("[AIChatPlugin] CaiBot plugin MessageHandle type not found.");
                    return Task.CompletedTask;  // Return directly
                }
                var sendDateAsyncMethod = messageHandleType.GetMethod("SendDateAsync", new[] { typeof(string) });  // Get CaiBot plugin SendDateAsync method
                if (sendDateAsyncMethod == null)  // CaiBot plugin SendDateAsync method does not exist
                {
                    TShock.Log.ConsoleError("[AIChatPlugin] CaiBot plugin SendDateAsync method not found.");
                    return Task.CompletedTask;  // Return directly
                }
                // Check if group number is valid
                if (Config.QQQL >= 10000 && Config.QQQL <= 999999999)  // Group number range is 6 to 9 digits, and starts from 10000
                {
                    var result = new  // Create CaiBot plugin MessageHandle type instance
                    {
                        type = "chat",
                        chat = $"[AI Chat Forward]\n{message}",
                        group = Config.QQQL,
                    };
                    string jsonMessage = JsonConvert.SerializeObject(result);  // Serialize CaiBot plugin MessageHandle type instance
                    var invokeResult = sendDateAsyncMethod.Invoke(null, new object[] { jsonMessage });  // Call CaiBot plugin SendDateAsync method
                    if (invokeResult == null)  // Call CaiBot plugin SendDateAsync method returns null
                    {
                        TShock.Log.ConsoleError("[AIChatPlugin] Called SendDateAsync method but returned null.");
                        return Task.CompletedTask;  // Return directly
                    }
                }
                else  // Invalid group number
                {
                    return Task.CompletedTask;  // Return directly
                }
            }
            catch (Exception ex)  // Error occurred while forwarding message to CaiBot plugin
            {
                TShock.Log.ConsoleError($"[AIChatPlugin] Error occurred while forwarding message to CaiBot plugin: {ex.Message}");
            }
            return Task.CompletedTask;  // Return directly
        }
        #endregion
    }
}

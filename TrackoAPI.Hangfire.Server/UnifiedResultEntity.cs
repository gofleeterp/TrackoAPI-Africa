namespace TrackoAPI.Hangfire.Server
{
    /// <summary>
    /// 接口统一返回格式
    /// </summary>
    public class UnifiedResultEntity<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int msg { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string msgbox { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T data { get; set; }

        /// <summary>
        /// 拓展参数1 整型
        /// </summary>
        public int ext_int { get; set; }

        /// <summary>
        /// 拓展参数2 string
        /// </summary>
        public string ext_str { get; set; }
    }
}

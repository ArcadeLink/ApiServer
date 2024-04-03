namespace ArcadeLinkOtpAuthProvider.Models;

public class BaiduServiceResponse
{
    /// <summary>
    /// 合规与否
    /// </summary>
    public string conclusion { get; set; }
    /// <summary>
    /// 日志ID
    /// </summary>
    public string log_id { get; set; }
    /// <summary>
    /// 是否命中
    /// </summary>
    public string isHitMd5 { get; set; }
    /// <summary>
    /// 命中的违规类型 1-合规
    /// </summary>
    public int conclusionType { get; set; }
}
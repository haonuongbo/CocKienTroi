/// <summary>
/// Interface chung cho tất cả các script điều khiển xe (player + AI).
/// CarItemManager chỉ cần tương tác qua interface này — không cần biết loại xe cụ thể.
/// </summary>
public interface ICarController
{
    /// <summary>Tốc độ tối đa hiện tại của xe.</summary>
    float MaxSpeed { get; set; }

    /// <summary>Hệ số trượt khi drift (0 = trượt tối đa, 1 = không trượt).</summary>
    float DriftSlide { get; set; }

    /// <summary>Bật/tắt toàn bộ input điều khiển xe.</summary>
    void SetControlEnabled(bool enabled);
}

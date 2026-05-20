namespace SensorX.Master.Application.Commands.Sepays
{
  public record  HandlerPaymentSepayCommand
  {
    public int Id { get; set; }
    public string? Gateway { get; set; }
    public string? TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? Code { get; set; }
    public string? Content { get; set; }              // Nội dung chuyển khoản
    public string? TransferType { get; set; }         // Loại giao dịch. in là tiền vào, out là tiền ra
    public decimal? TransferAmount { get; set; }      // Số tiền giao dịch
    public decimal? Accumulated { get; set; }         // Số dư tài khoản (lũy kế)
    public string? SubAccount { get; set; }           // Tài khoản ngân hàng phụ (tài khoản định danh)
    public string? ReferenceCode { get; set; }        // Mã tham chiếu của tin nhắn sms
    public string? Description { get; set; }          // Toàn bộ nội dung tin nhắn sms
  }
}

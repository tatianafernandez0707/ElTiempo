namespace ApiElTiempo.Models.CtlErr
{
    public class ReturnData
    {
        public bool IsSuccess { get; set; } 
        public string Message { get; set; }
        public object? Data { get; set; }
    }
}

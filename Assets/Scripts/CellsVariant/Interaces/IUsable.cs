public interface IUsable
{
    string Effect_Url {get;set;}
    bool Active { get; set; }
    bool IsReadyToUse { get; }

    void Use();
}
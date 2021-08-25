public interface IUsable
{
    string Effect_Url {get;set;}
    bool IsReadyToUse { get; }
    bool IsUsed { get; }

    void Use();
}
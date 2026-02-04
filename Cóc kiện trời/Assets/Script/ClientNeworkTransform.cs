
using Unity.Netcode.Components;
public class ClientNeworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return true;
    }
    
}

namespace ET.Client.Platform
{
    public interface IGameContext
    {
        IEffectControl EffectControl { get; }
        ISoundControl SoundControl { get; }
    }
}
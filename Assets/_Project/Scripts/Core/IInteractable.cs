namespace TimeMachine.Core
{
    public interface IInteractable
    {
        string InteractionId { get; }
        string PromptLabel { get; }
        bool CanInteract { get; }
        void Interact(string actorId);
    }
}

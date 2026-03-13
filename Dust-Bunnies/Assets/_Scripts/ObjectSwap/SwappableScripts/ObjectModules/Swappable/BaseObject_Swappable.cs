public partial class BaseObject
{
    /// <summary>
    /// Event fired when Swappable performed a successful state swap;
    /// </summary>
    public event System.Action<SwapState> OnSwapStateSet;

    /// <summary>
    /// Event fired before Swappable confirms a state swap, if any;<br>
    /// </br> Used by the Swappable module. Use <b>OnSwapStateSet</b> for a proper confirmation;
    /// </summary>
    public event System.Func<SwapState, bool> OnTrySwapState;

    /// <summary>
    /// Try to set the object's swap state to the given one, if possible;
    /// </summary>
    /// <param name="swapState"> Swap state to be set; </param>
    /// <returns> True if the state was swapped successfully, false otherwise; </returns>
    public bool TrySwapState(SwapState swapState) {
        return OnTrySwapState?.Invoke(swapState) ?? false;
    }

    /// <summary>
    /// Trigger for a successful swap state update;<br>
    /// </br> Used by the Swappable module, <b>do not use outside of said context</b>;
    /// </summary>
    public void PropagateSwapState(SwapState swapState) {
        OnSwapStateSet?.Invoke(swapState);
    }
}

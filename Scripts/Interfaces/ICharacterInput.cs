
public interface ICharacterInput
{
    ///<summary> Direction character desires to move on the horizontal axis.</summary>
    float horizontal {get;}   

    ///<summary> Direction character desires to move on the vertical axis.</summary>
    float vertical {get;}

    ///<summary> Is the sprint key being held down? </summary>
    bool sprintKeyPressed{get;}

    ///<summary> Is the fire key being held down? </summary>
    bool fireKeyPressed{get;}

    ///<summary> Is the block key being held down? </summary>
    bool blockKeyPressed {get;}


    ///<summary> The amount by which the character wants to move his mouse horizontally.</summary>
    float horizontalMouse{get;}


    ///<summary> Invoked when the character desires to swing.</summary>
    event System.Action OnJumpKeyPressed;    // Although Swing() is a public method, we will leave the option open for the input module to broadcast an OnSwing event (to keep the input module decoupled from the swing module)


    event System.Action OnFireStart;
    event System.Action OnFireEnd;

}

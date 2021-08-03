using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageStrings : MonoBehaviour
{
    public static LanguageStrings Instance;
    public List<string> texts;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        texts.Add("Please wait for dispute to resolve");
        texts.Add("You already have opened channel.");
        texts.Add("CREATE CHANNEL. Please wait...");
        texts.Add("Already joining a channel");
        texts.Add("Please wait for dispute to resolve");
        texts.Add("JOIN CHANNEL.(");
        texts.Add(".Please wait...");
        texts.Add("CLOSE CHANNEL. Please wait...");
        texts.Add("You must position your ships!");
        texts.Add("There is dispute!");
        texts.Add("You are the loser according to the resolved dispute");
        texts.Add("You are the winner according to the resolved dispute");
        texts.Add("YOUR TURN!");
        texts.Add("GAME FINISHED! ");
        texts.Add("You have won.");
        texts.Add("You have lost.");
        texts.Add("You already submited ship's positions.");
        texts.Add("Some ships do not point!\nYou can't submit ship's postitions."); //if (GlobalData.gGameControl.gameMyBoard.CountOfShips()< 7)
        texts.Add("Positions of ships do not validate!\nYou can't submit ship's postitions.");
        texts.Add("START GAME.\n ARRANGE YOUR SHIPS!");
        texts.Add("No channel is opened to dispute it");
        texts.Add("Please wait for dispute to resolve");
        texts.Add("You started a dispute!");
        texts.Add("Positions submited"); //23
    }
}

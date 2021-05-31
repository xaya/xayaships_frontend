using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAYA
{
    public class ElectrumWalletHandler : MonoBehaviour
    {

        public InputField generateNewAddressField;
        public InputField enterNewNameField;
        public InputField sendAmountField;
        public InputField sendAddressField;
        public InputField seedPhraseRevealedInputFIeld;

        public GameObject seedPhrasePanel;
        public Text statusText;

        public GameObject sendChiConfirmationPanel;
        public Text sendChiConfirmationText;

        private double cachedSendAmout;
        private string cachedSendAddress;

        IEnumerator SolveReceiveAddressIfPresent()
        {
            RPCRequest r = new RPCRequest();
            ConnectionStatusSolver connectionSolver = GameObject.FindObjectOfType<ConnectionStatusSolver>();

            while (connectionSolver.walletSolved == false)
            {
                yield return new WaitForSeconds(0.5f);
            }

            List<List<string>> addresses = r.GetAddressList();

            string ourAddress = PlayerPrefs.GetString("receiveaddress", "");

            bool addresIsInList = false;

            if (addresses != null)
            {
                for (int s = 0; s < addresses.Count; s++)
                {
                    if (addresses[s].Contains(ourAddress))
                    {
                        addresIsInList = true;
                        break;
                    }
                }
            }

            if (addresIsInList)
            {
                generateNewAddressField.text = generateNewAddressField.text = PlayerPrefs.GetString("receiveaddress", ""); ;
            }
        }

        void Start()
        {
            StartCoroutine(SolveReceiveAddressIfPresent());
        }

        public void GenerateNewAddress()
        {
            RPCRequest r = new RPCRequest();
            generateNewAddressField.text = r.GetNewAddress();

            PlayerPrefs.SetString("receiveaddress", generateNewAddressField.text);
        }

        public void CloseSeedPhrase()
        {
            seedPhrasePanel.SetActive(false);
        }

        public void ViewSeedPhrase()
        {
            seedPhrasePanel.SetActive(true);
        }

        public void ViewSeedPhraseYes()
        {
            StartCoroutine(ShowSeedEnum());
        }

        IEnumerator ShowSeedEnum()
        {
            RPCRequest r = new RPCRequest();
            seedPhraseRevealedInputFIeld.text = r.ElectrumWalletGetSeed();
            seedPhraseRevealedInputFIeld.gameObject.SetActive(true);

            yield return null;
        }

        public void ViewSeedPhraseNo()
        {
            seedPhrasePanel.SetActive(false);
        }

        public void AddNewName()
        {
            RPCRequest r = new RPCRequest();

            if (enterNewNameField.text != "")
            {
                r.XAYANameRegister(enterNewNameField.text);
                statusText.text = "The name will appear in name selection list next block";
                XAYADummyUI.Instance.waitingForNewName = 5.0f;
            }
        }

        public void SendCHI()
        {
            double amountToSent = 0;

            double.TryParse(sendAmountField.text, out amountToSent);

            if (amountToSent > 0 && sendAddressField.text != "")
            {
                cachedSendAmout = amountToSent;
                cachedSendAddress = sendAddressField.text;

                sendChiConfirmationPanel.SetActive(true);
                sendChiConfirmationText.text = "Do you really want to send " + cachedSendAmout + " to address " + cachedSendAddress + "?";
            }
        }


        public void SendCHINO()
        {
            sendChiConfirmationPanel.SetActive(false);
        }

        public void SendCHIYES()
        {
            RPCRequest r = new RPCRequest();

            r.SendToAddress(cachedSendAddress, cachedSendAmout);
            statusText.text = "Send TX submited to chain";
            sendAddressField.text = "";
            cachedSendAmout = 0;
            PanelClose();
        }

        public void PanelClose()
        {
            //hide wallet in real game UI
            XAYADummyUI.Instance.HideLiteWallet();
        }

        public void PanelShow()
        {
            //show wallet in real game UI
        }
    }
}
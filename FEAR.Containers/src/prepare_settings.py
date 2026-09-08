import json
import uuid
from . import prepare_config_file
from . import generate_key_set

def prepare_appsettings_file(DEPLOY_FOLDER, deploy_config_data):
    result = generate_key_set()
    prepare_config_file("hosted-keyconfig.json", DEPLOY_FOLDER, "FEAR", lambda config_data: {
        config_data.update({ "Keys": [
            {
                "KeyName": result["KeyName"],
                "KeyIdentifiers": result["KeyIdentifiers"],
                "EncryptedKey": result["EncryptedKey"]
            }
        ]})
    })

    def hosted_appsetting_updater(config_data, deploy_config_data):
        config_data["Initialization"]["AutoDecryptSet"] = [{
                    "KeyName": result["KeyName"],
                    "Values": {
                        "PrimaryKey": result["KeySecrets"]["PrimaryKey"],
                        "SecondaryKey": result["KeySecrets"]["SecondaryKey"]
                    }
                }]
        
        config_data["Authentication"]["EncryptionKeyName"] = result["KeyName"]

    def wasm_appsettings_updater(config_data, deploy_config_data):
        config_data["Local"]["Authority"] = f"https://{deploy_config_data['FEAR_API_HOSTNAME']}"
        config_data["Local"]["PostLogoutRedirectUri"] = f"https://{deploy_config_data['FEAR_API_HOSTNAME']}/logout-callback"
        config_data["Local"]["RedirectUri"] = f"https://{deploy_config_data['FEAR_API_HOSTNAME']}/login-callback"

    prepare_config_file("hosted-appsettings.json", DEPLOY_FOLDER, "FEAR", lambda config_data: hosted_appsetting_updater(config_data, deploy_config_data))
    prepare_config_file("wasm-appsettings.json", DEPLOY_FOLDER, "FEAR", lambda config_data: wasm_appsettings_updater(config_data, deploy_config_data))
    prepare_config_file("admin-appsettings.json", DEPLOY_FOLDER, "FEAR", lambda config_data: wasm_appsettings_updater(config_data, deploy_config_data))


def prepare_initial_cases_file(DEPLOY_FOLDER, deploy_config_data, responses):
    def updater(config_data, responses):
        for item in config_data:
            item["Id"] = str(uuid.uuid4())

            hasRemote = False
            remoteName = ""
            packages = []
            if item["InvestigationName"] == "Malware-Eval":
                hasRemote = True
                remoteName = "malware-eval"
                packages = [f"https://scripts.{responses['TLD_NAME']['input']}/Packages/Malware-Eval.zip"]

            if item["InvestigationName"] == "Remote Graph Case":
                hasRemote = True
                remoteName = "remote-graph-case"

            if hasRemote:
                item["Connections"]["GraphDBConnection"]["Endpoint"] = f"http://jena:3030/{remoteName}/data"
                item["Connections"]["GraphDBConnection"]["Username"] = f"admin"
                item["Connections"]["GraphDBConnection"]["Password"] = responses["SYSTEM_PASSWORD"]["input"]

            item["PackagedSources"] = packages

            item["AgentOptions"]["Enabled"] = responses["LLM_CONFIGURE"]["input"]

            if responses["LLM_CONFIGURE"]["input"] == True:
                item["AgentOptions"]["DefaultAgent"] = "FEAR-DefaultAgent"
                item["AgentOptions"]["AgentOrder"] = ["FEAR-DefaultAgent"]
                item["AgentOptions"]["Agents"] = { 
                    "FEAR-DefaultAgent": {
                        "AgentId": "AG-" + str(uuid.uuid4()), 
                        "AgentName": "FEAR-DefaultAgent", 
                        "AgentType": responses["LLM_TYPE"]["input"], 
                        "Description": "Default agent for FEAR", 
                        "InputChain": [ "\u003CPrompt\u003E" ],
                        "Description":"",
                        "AgentOptions":{ 
                            "ApiKey": responses["LLM_API_KEY"]["input"], 
                            "Model": responses["LLM_MODEL"]["input"],
                            "AgentBaseUrl": responses["LLM_API_URL"]["input"] if responses["LLM_TYPE"]["input"] != "OpenAI" else None
                        }
                    } 
                }

    prepare_config_file("initial_cases.json", DEPLOY_FOLDER, "FEAR", lambda config_data: updater(config_data, responses))
    prepare_config_file("initial_color_maps.json", DEPLOY_FOLDER, "FEAR", lambda config_data: {})


def prepare_case_creation_defaults_file(DEPLOY_FOLDER, deploy_config_data, responses):
    def updater(item, responses):
        packages = [f"https://scripts.{responses['TLD_NAME']['input']}/Packages/Malware-Eval.zip"]

        item["PackagedSources"] = packages

        item["AgentOptions"]["Enabled"] = responses["LLM_CONFIGURE"]["input"]
        if responses["LLM_CONFIGURE"]["input"] == True:
            item["AgentOptions"]["DefaultAgent"] = "FEAR-DefaultAgent"
            item["AgentOptions"]["AgentOrder"] = ["FEAR-DefaultAgent"]
            item["AgentOptions"]["Agents"] = { 
                "FEAR-DefaultAgent": {
                    "AgentId": "", 
                    "AgentName": "FEAR-DefaultAgent", 
                    "AgentType": responses["LLM_TYPE"]["input"], 
                    "AgentModel": responses["LLM_MODEL"]["input"],
                    "Description": "Default agent for FEAR", 
                    "InputChain": [ "\u003CPrompt\u003E" ],
                    "Description":""
                }
            }
            item["AgentOptions"]["AgentOptions"] = { 
                "FEAR-DefaultAgent": { 
                    "ApiKey": responses["LLM_API_KEY"]["input"], 
                    "AgentBaseUrl": responses["LLM_API_URL"]["input"] if responses["LLM_TYPE"]["input"] != "OpenAI" else None,
                    "SystemPrompt": None, 
                    "SystemPromptFile": None 
                    }
                }

    prepare_config_file("case_creation_defaults.json", DEPLOY_FOLDER, "FEAR", lambda config_data: updater(config_data, responses))

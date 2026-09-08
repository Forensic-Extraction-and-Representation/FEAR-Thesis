from argparse import ArgumentParser
import os
import posixpath
import shutil
import sys
import json
from urllib import response
import uuid

BUILD_FOLDER = os.path.join(os.getcwd(), "..", "build")

from src import generate_root_ca, generate_intermediate_ca, generate_ssl_certificate, generate_rsa_private_key
from src import ask_questions
from src import generate_ca_bundle, validate_ca_bundle
from src import generate_key_set, random_uuid_string, random_string, copy_file
from src import prepare_deploy_directory_certificates, prepare_nginx_files, prepare_appsettings_file, prepare_initial_cases_file, generate_hosted_secrets, prepare_case_creation_defaults_file
from src import apply_deploy_overlay

questions = [ 
    { "Name": "CONFIG_ONLY", "Question": "Only prepare config files and do not build containers?", "Default": "No", "Type": "boolean" },
    { "Name": "TLD_NAME", "Question": "Enter the top level domain name. 4 sub-domains will be used (api, ui, scripts, dbs)", "Default":"fear.local", "Type": "string", 
     "Action": lambda responses: responses.update({"TLD_NAME": {"input": responses["TLD_NAME"]["input"].lower()}}) },
    { "Name": "SYSTEM_PASSWORD", "Question": "System password?", "Type": "password", "Default": "password"},
    { "Name": "GEN_CA_BUNDLE", "Question": "Generate CA Bundle?", "Default": "Yes", "Type": "boolean", "SubQuestionSets": [{
           "Conditions": [ lambda responses: responses["GEN_CA_BUNDLE"]["input"] == True],
           "Questions": [
                { "Name": "CA_PRIVATE_KEY_FILE", "Question": "CA Private Key file name?", "Type": "string", "Default": "fear-root.key" },
                { "Name": "CA_CERT_FILE", "Question": "CA Certificate file name?", "Type": "string", "Default": "fear-root.crt" },
                { "Name": "CA_PASSWORD", "Question": "CA Password?", "Default": "password", "Type": "password" },
                { "Name": "CA_COUNTRY", "Question": "CA Country?", "Type": "string", "Default": "AU" },
                { "Name": "CA_STATE", "Question": "CA State?", "Type": "string", "Default": "WA" },
                { "Name": "CA_LOCALITY", "Question": "CA Locality?", "Type": "string", "Default": "Perth" }
            ],
           "Name": "CA_BUNDLE_DETAILS",
           "Action": lambda responses: generate_ca_bundle(responses, BUILD_FOLDER)
        },
        {
            "Conditions": [ lambda responses: responses["GEN_CA_BUNDLE"]["input"] == False],
            "Questions": [
                { "Name": "FEAR_API_PRIVATE_KEY_FILE", "Question": "CA Private Key file name?", "Type": "string", "Default": "api.fear.app.key" },
                { "Name": "FEAR_API_CERT_FILE", "Question": "CA Certificate file name?", "Type": "string", "Default": "api.fear.app.crt" },
                { "Name": "FEAR_API_PASSWORD", "Question": "CA Password?", "Type": "password" },
                { "Name": "FEAR_UI_PRIVATE_KEY_FILE", "Question": "UI Private Key file name?", "Type": "string", "Default": "ui.fear.app.key" },
                { "Name": "FEAR_UI_CERT_FILE", "Question": "UI Certificate file name?", "Type": "string", "Default": "ui.fear.app.crt" },
                { "Name": "FEAR_UI_PASSWORD", "Question": "UI Password?", "Type": "password" },
                { "Name": "FEAR_SCRIPTS_PRIVATE_KEY_FILE", "Question": "Scripts Private Key file name?", "Type": "string", "Default": "scripts.fear.app.key" },
                { "Name": "FEAR_SCRIPTS_CERT_FILE", "Question": "Scripts Certificate file name?", "Type": "string", "Default": "scripts.fear.app.crt" },
                { "Name": "FEAR_SCRIPTS_PASSWORD", "Question": "Scripts Password?", "Type": "password" },
                { "Name": "FEAR_DBS_PRIVATE_KEY_FILE", "Question": "DBS Private Key file name?", "Type": "string", "Default": "dbs.fear.app.key" },
                { "Name": "FEAR_DBS_CERT_FILE", "Question": "DBS Certificate file name?", "Type": "string", "Default": "dbs.fear.app.crt" },
                { "Name": "FEAR_DBS_PASSWORD", "Question": "DBS Password?", "Default": "password", "Type": "password" }
            ],
           "Name": "CA_BUNDLE_DETAILS"
        }],
        "Action": lambda responses: validate_ca_bundle(responses, BUILD_FOLDER)
    },
    { "Name": "EXTRA_CA_CERTS_DIRECTORY", "Question": "Directory containing any additional CA certs to trust?", "Type": "string", "Default": "" },
    { "Name": "LLM_CONFIGURE", "Question": "Do you have an OpenAI or OpenWebAI (ie, self-hosted) API key?", "Type": "boolean", "Default": "True",
        "SubQuestionSets": [{
                "Conditions": [ lambda responses: responses["LLM_CONFIGURE"]["input"] == True],
                "Questions": [
                    { "Name": "LLM_TYPE", "Question": "Which type of LLM do you have?", "Type": "string", "ValidValues": ["OpenAI", "OpenWebAI"], "Default": "OpenAI" },
                    { "Name": "LLM_MODEL", "Question": "Which model do you want to use?", "Type": "string", "Default": "gpt-4o" },
                    { "Name": "LLM_API_KEY", "Question": "Enter your LLM API key", "Type": "password", "Default": "" },
                    { "Name": "LLM_API_URL", "Question": "Enter your LLM API URL (only for OpenWebAI or other non-OpenAI providers)", "Type": "string", "Default": "",
                      "Conditions": [ lambda responses: responses["LLM_TYPE"]["input"] != "OpenAI"] 
					}
                ],
                "Name": "LLM_DETAILS"
            }]
     }
]


def main():
    global BUILD_FOLDER
    docker_args = []
    deploy_config_data = {}

    arg_parser = ArgumentParser(description="FEAR Application Container Builder")
    arg_parser.add_argument("--build-folder", type=str, default=BUILD_FOLDER, help="Folder to store build artifacts")
    args = arg_parser.parse_args()
    
    BUILD_FOLDER = os.path.realpath(args.build_folder)
    DEPLOY_FOLDER = os.path.join(os.getcwd(), "..", "deploy")
    DEPLOY_OVERLAY_FOLDER = os.path.join(os.getcwd(), "..", "deploy_overlay")
    SECRETS_FOLDER = os.path.join(os.getcwd(), "..","secrets")

    if not os.path.exists(BUILD_FOLDER):
        os.makedirs(BUILD_FOLDER, exist_ok=True)
        
    if not os.path.exists(DEPLOY_FOLDER):
        os.makedirs(DEPLOY_FOLDER, exist_ok=True)

    if not os.path.exists(SECRETS_FOLDER):
        os.makedirs(SECRETS_FOLDER, exist_ok=True)
       
    print("Welcome to the FEAR Application Container Builder!")
    print("Build folder is set to: " + BUILD_FOLDER)

    # Ask questions to gather necessary information for building the container
    responses = {}
    ask_questions(questions, responses)

    prepare_deploy_directory_certificates(responses, deploy_config_data, docker_args, DEPLOY_FOLDER, BUILD_FOLDER)
    
    deploy_config_data["SYSTEM_PASSWORD"] = responses["SYSTEM_PASSWORD"]["input"]
    deploy_config_data["FEAR_API_HOSTNAME"] = f"api.{responses['TLD_NAME']['input']}"
    deploy_config_data["FEAR_UI_HOSTNAME"] = f"ui.{responses['TLD_NAME']['input']}"
    deploy_config_data["FEAR_SCRIPTS_HOSTNAME"] = f"scripts.{responses['TLD_NAME']['input']}"
    deploy_config_data["FEAR_DBS_HOSTNAME"] = f"dbs.{responses['TLD_NAME']['input']}"

    # Many of the configuration files will retain the value of "<Replaced>" for sensitive properties as they are
    # injected via the docker-compose.yml file and the secrets files. 
    # This is to avoid sensitive information being stored in the configuration files.
    prepare_appsettings_file(DEPLOY_FOLDER, deploy_config_data)
    prepare_initial_cases_file(DEPLOY_FOLDER, deploy_config_data, responses)
    prepare_case_creation_defaults_file(DEPLOY_FOLDER, deploy_config_data, responses)
    generate_hosted_secrets(SECRETS_FOLDER, deploy_config_data, responses)

    prepare_nginx_files(responses, DEPLOY_FOLDER)

    apply_deploy_overlay(DEPLOY_OVERLAY_FOLDER, DEPLOY_FOLDER)
    
    jena_env_file = os.path.join(DEPLOY_FOLDER, "jena.env")
    with open(jena_env_file, "w") as f:
        f.write(f"ADMIN_PASSWORD={responses['SYSTEM_PASSWORD']['input']}")
    
    db_secrets_file = os.path.join(SECRETS_FOLDER, "db_password")
    with open(db_secrets_file, "w") as f:
        f.write(responses["SYSTEM_PASSWORD"]["input"])
        
    print("To ensure you can access the sites, you may need to add the following entries to your hosts file:")
    print(f"127.0.0.1 {deploy_config_data['FEAR_API_HOSTNAME']}")
    print(f"127.0.0.1 {deploy_config_data['FEAR_UI_HOSTNAME']}")
    print(f"127.0.0.1 {deploy_config_data['FEAR_SCRIPTS_HOSTNAME']}")
    print(f"127.0.0.1 {deploy_config_data['FEAR_DBS_HOSTNAME']}")

    if(responses["CONFIG_ONLY"]["input"] == True):
        print("Configuration files prepared in deploy folder. Exiting without building containers.")
        return
    else:
        print("Building containers and starting application with docker compose...")
        os.system(f"docker build --progress=plain -t fear-app --build-arg TLD={responses['TLD_NAME']['input']} -f ./FEAR/Dockerfile ../")
        os.system(f"docker build --progress=plain -t fear-dbs -f ./Postgres/Dockerfile ../")
    
        project_name = f"fearapp_{responses['TLD_NAME']['input']}".replace(".", "-")
        override_file = os.path.join(DEPLOY_OVERLAY_FOLDER, "docker-compose.override.yml")
        override_arg = f" -f ./docker-compose.override.yml" if os.path.exists(override_file) else ""
        if override_arg:
            shutil.copy2(override_file, os.path.join(os.getcwd(), "docker-compose.override.yml"))
            print("[overlay] docker-compose.override.yml applied")
        os.system(f"docker compose -p {project_name} --project-directory ../ -f ./docker-compose.yaml{override_arg} up")

if __name__ == "__main__":
    sys.exit(int(main() or 0))
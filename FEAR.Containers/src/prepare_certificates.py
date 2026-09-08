import os
from . import copy_file

def prepare_deploy_directory_certificates(responses, deploy_config_data, docker_args, DEPLOY_FOLDER, BUILD_FOLDER):
    deploy_config = os.path.join(DEPLOY_FOLDER, "deploy_config.json")

    dir_list = [ os.path.join("ssl", "certs"), os.path.join("ssl", "private"), os.path.join("ssl", "extra") ]
    for dir in dir_list:
        os.makedirs(os.path.join(DEPLOY_FOLDER, dir), exist_ok=True)

    ca_certs = ["CA_CERT_FILE", "FEAR_SYS_INT_CERT_FILE"]
    deploy_config_data.update({key: responses[key]["input"] for key in ca_certs if key in responses})

    # Copy the relevant files to the deploy folder
    for key in ca_certs:
        if key in responses:
            src = os.path.join(BUILD_FOLDER, responses[key]["input"])
            dst = os.path.join(DEPLOY_FOLDER, "ssl", "extra", os.path.basename(src))
            copy_file(src, dst)
            deploy_config_data[key] = dst

    # Define the SSL files to copy and include in the deploy config
    certs = ["FEAR_API_CERT_FILE", "FEAR_UI_CERT_FILE", "FEAR_SCRIPTS_CERT_FILE", "FEAR_DBS_CERT_FILE"]
    deploy_config_data.update({key: responses[key]["input"] for key in certs if key in responses})

    # Copy the relevant files to the deploy folder
    for key in certs:
        if key in responses:
            src = os.path.join(BUILD_FOLDER, responses[key]["input"])
            dst = os.path.join(DEPLOY_FOLDER, "ssl", "certs", os.path.basename(src))
            copy_file(src, dst)
            deploy_config_data[key] = dst
    
    priv_keys = ["FEAR_API_PRIVATE_KEY_FILE", "FEAR_UI_PRIVATE_KEY_FILE", "FEAR_SCRIPTS_PRIVATE_KEY_FILE", "FEAR_DBS_PRIVATE_KEY_FILE"]
    deploy_config_data.update({key: responses[key]["input"] for key in priv_keys if key in responses})
    for key in priv_keys:
        if key in responses:
            src = os.path.join(BUILD_FOLDER, responses[key]["input"])
            dst = os.path.join(DEPLOY_FOLDER, "ssl", "private", os.path.basename(src))
            copy_file(src, dst)
            deploy_config_data[key] = dst

    # Include any extra CA certs if specified
    if responses["EXTRA_CA_CERTS_DIRECTORY"]["input"]:
        extra_certs_dir = responses["EXTRA_CA_CERTS_DIRECTORY"]["input"]
        deploy_config_data["EXTRA_CA_CERTS"] = []
        for filename in os.listdir(extra_certs_dir):
            src = os.path.join(extra_certs_dir, filename)
            dst = os.path.join(DEPLOY_FOLDER, "ssl", filename)
            copy_file(src, dst)
        
        deploy_config_data["EXTRA_CA_CERTS_DIR"] = os.path.join(DEPLOY_FOLDER, "ssl", "extra")
    # Write the deploy config file
    with open(deploy_config, "w") as f:
        import json
        json.dump(deploy_config_data, f, indent=4)

import json
import os

def prepare_config_file(filename, DEPLOY_FOLDER, source, update_lambda):
    # Open the file and read the contents so it can be modified and writtent to the deploy folder
    config_file = os.path.join(source, filename)
    with open(config_file, "r") as f:
        config_data = json.load(f)
    # Placeholder for update commands to the json data
    update_lambda(config_data)
    # Write the modified appsettings file to the deploy folder
    deploy_config_file = os.path.join(DEPLOY_FOLDER, filename)
    with open(deploy_config_file, "w") as f:
        json.dump(config_data, f, indent=4)


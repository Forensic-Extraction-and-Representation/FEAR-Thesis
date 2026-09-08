import os
from . import random_uuid_string, generate_rsa_private_key

def generate_hosted_secrets(SECRETS_FOLDER, deploy_config_data, responses):
    with open(os.path.join(SECRETS_FOLDER, "ConnectionStrings__HostedSystemConnection"), "w") as f:
        f.write(f"Server=postgres;Port=5432;Database=hosted_system;User Id=fear_user;Password={deploy_config_data['SYSTEM_PASSWORD']};")

    with open(os.path.join(SECRETS_FOLDER, "ConnectionStrings__InvestigationConnection"), "w") as f:
        f.write(f"Server=postgres;Port=5432;Database=investigation;User Id=fear_user;Password={deploy_config_data['SYSTEM_PASSWORD']};")

    with open(os.path.join(SECRETS_FOLDER, "ConnectionStrings__IdentityConnection"), "w") as f:
        f.write(f"Server=postgres;Port=5432;Database=identity;User Id=fear_user;Password={deploy_config_data['SYSTEM_PASSWORD']};")

    with open(os.path.join(SECRETS_FOLDER, "ConnectionStrings__OpenIdConnection"), "w") as f:
        f.write(f"Server=postgres;Port=5432;Database=openid;User Id=fear_user;Password={deploy_config_data['SYSTEM_PASSWORD']};")
    
    with open (os.path.join(SECRETS_FOLDER, "Jwt__Key") , "w") as f:
        f.write(random_uuid_string(128))

    with open (os.path.join(SECRETS_FOLDER, "Jwt__EncKey") , "w") as f:
        f.write(random_uuid_string(32))
    
    with open (os.path.join(SECRETS_FOLDER, "ChatInference__SigningKey") , "w") as f:
        f.write(generate_rsa_private_key())

    with open (os.path.join(SECRETS_FOLDER, "SystemTld") , "w") as f:
        f.write(responses["TLD_NAME"]["input"])


import os
from .crypto_helpers import generate_root_ca, generate_intermediate_ca, generate_ssl_certificate

def generate_ca_bundle(responses, BUILD_FOLDER):
    print("Generating CA Bundle...")
    certificate_password = responses["CA_PASSWORD"]["input"].encode()
    ca_private_key = os.path.join(BUILD_FOLDER, responses["CA_PRIVATE_KEY_FILE"]["input"])
    ca_public_cert = os.path.join(BUILD_FOLDER, responses["CA_CERT_FILE"]["input"])
    generate_root_ca(responses["CA_COUNTRY"]["input"], responses["CA_STATE"]["input"], responses["CA_LOCALITY"]["input"], 
                    ca_private_key, ca_public_cert, certificate_password)    

    int_private_key = os.path.join(BUILD_FOLDER, "fear.sys.int.key")
    int_public_cert = os.path.join(BUILD_FOLDER, "fear.sys.int.crt")
    generate_intermediate_ca(responses["CA_COUNTRY"]["input"], responses["CA_STATE"]["input"], responses["CA_LOCALITY"]["input"], 
                             ca_private_key, ca_public_cert,
                             int_private_key, int_public_cert, certificate_password)

    responses["FEAR_SYS_INT_PRIVATE_KEY_FILE"] = {"input": int_private_key}
    responses["FEAR_SYS_INT_CERT_FILE"] = {"input": int_public_cert}
    responses["FEAR_SYS_INT_PASSWORD"] = {"input": certificate_password}

    api_private_key = os.path.join(BUILD_FOLDER, "api.fear.app.key")
    api_public_cert = os.path.join(BUILD_FOLDER, "api.fear.app.crt")
    generate_ssl_certificate(int_private_key, int_public_cert, certificate_password, BUILD_FOLDER, f"api.{responses['TLD_NAME']['input']}", f"api.fear.app")
    responses["FEAR_API_PRIVATE_KEY_FILE"] = {"input": api_private_key}
    responses["FEAR_API_CERT_FILE"] = {"input": api_public_cert}

    ui_private_key = os.path.join(BUILD_FOLDER, "ui.fear.app.key")
    ui_public_cert = os.path.join(BUILD_FOLDER, "ui.fear.app.crt")
    generate_ssl_certificate(int_private_key, int_public_cert, certificate_password, BUILD_FOLDER, f"ui.{responses['TLD_NAME']['input']}", f"ui.fear.app")
    responses["FEAR_UI_PRIVATE_KEY_FILE"] = {"input": ui_private_key}
    responses["FEAR_UI_CERT_FILE"] = {"input": ui_public_cert}

    scripts_private_key = os.path.join(BUILD_FOLDER, "scripts.fear.app.key")
    scripts_public_cert = os.path.join(BUILD_FOLDER, "scripts.fear.app.crt")
    generate_ssl_certificate(int_private_key, int_public_cert, certificate_password, BUILD_FOLDER, f"scripts.{responses['TLD_NAME']['input']}", f"scripts.fear.app")
    responses["FEAR_SCRIPTS_PRIVATE_KEY_FILE"] = {"input": scripts_private_key}
    responses["FEAR_SCRIPTS_CERT_FILE"] = {"input": scripts_public_cert}

    dbs_private_key = os.path.join(BUILD_FOLDER, "dbs.fear.app.key")
    dbs_public_cert = os.path.join(BUILD_FOLDER, "dbs.fear.app.crt")
    generate_ssl_certificate(int_private_key, int_public_cert, certificate_password, BUILD_FOLDER, f"dbs.{responses['TLD_NAME']['input']}", f"dbs.fear.app")
    responses["FEAR_DBS_PRIVATE_KEY_FILE"] = {"input": dbs_private_key}
    responses["FEAR_DBS_CERT_FILE"] = {"input": dbs_public_cert}

def validate_ca_bundle(responses, BUILD_FOLDER):
    pass;


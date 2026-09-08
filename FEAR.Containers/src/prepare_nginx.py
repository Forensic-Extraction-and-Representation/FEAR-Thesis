import os
import posixpath

def prepare_nginx_files(responses, DEPLOY_FOLDER):
    cert_base_path = "/etc/ssl/certs"
    key_base_path = "/etc/ssl/private"

    api_site_config = '''
    server {
        listen  443 ssl;
        server_name api.<hostname>;

        ssl_certificate <ssl_certificate>;
        ssl_certificate_key <ssl_certificate_key>;

        client_max_body_size 256M;

        root /var/www/api;
        location / {
                proxy_pass         https://localhost:8009;
                proxy_http_version 1.1;
                proxy_set_header   Upgrade $http_upgrade;
                proxy_set_header   Connection keep-alive;
                proxy_set_header   Host $host;
                proxy_cache_bypass $http_upgrade;
                proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_set_header   X-Forwarded-Proto $scheme;
        }
    }
    ''' \
        .replace("<hostname>", responses["TLD_NAME"]["input"]) \
        .replace("<ssl_certificate>", posixpath.join(cert_base_path, os.path.basename(responses["FEAR_API_CERT_FILE"]["input"]))) \
        .replace("<ssl_certificate_key>", posixpath.join(key_base_path, os.path.basename(responses["FEAR_API_PRIVATE_KEY_FILE"]["input"])))


    scripts_site_config = '''
    server {
        listen  443 ssl;
        server_name scripts.<hostname>;

        ssl_certificate <ssl_certificate>;
        ssl_certificate_key <ssl_certificate_key>;

        root /var/www/fear-scripts;
        location / {
                try_files $uri $uri/ =404;
        }
    }
    ''' \
        .replace("<hostname>", responses["TLD_NAME"]["input"]) \
        .replace("<ssl_certificate>", posixpath.join(cert_base_path, os.path.basename(responses["FEAR_SCRIPTS_CERT_FILE"]["input"]))) \
        .replace("<ssl_certificate_key>", posixpath.join(key_base_path, os.path.basename(responses["FEAR_SCRIPTS_PRIVATE_KEY_FILE"]["input"])))

    ui_site_config = '''
    server {
        listen  443 ssl;
        server_name ui.<hostname>;

        ssl_certificate <ssl_certificate>;
        ssl_certificate_key <ssl_certificate_key>;

        root /var/www/fear-wasm;
        location / {
                try_files $uri $uri/ /index.html =404;
        }
    }
    ''' \
        .replace("<hostname>", responses["TLD_NAME"]["input"]) \
        .replace("<ssl_certificate>", posixpath.join(cert_base_path,os.path.basename(responses["FEAR_UI_CERT_FILE"]["input"]))) \
        .replace("<ssl_certificate_key>", posixpath.join(key_base_path, os.path.basename(responses["FEAR_UI_PRIVATE_KEY_FILE"]["input"])))

    # Make the nginx config directory and write the config files
    nginx_config_dir = os.path.join(DEPLOY_FOLDER, "nginx")
    os.makedirs(nginx_config_dir, exist_ok=True)

    with open(os.path.join(nginx_config_dir, f"api.{responses['TLD_NAME']['input']}"), "w") as f:
        f.write(api_site_config)

    with open(os.path.join(nginx_config_dir, f"scripts.{responses['TLD_NAME']['input']}"), "w") as f:
        f.write(scripts_site_config)

    with open(os.path.join(nginx_config_dir, f"ui.{responses['TLD_NAME']['input']}"), "w") as f:
        f.write(ui_site_config)



import os
import shutil
import patch_ng as patch_lib


def copy_file(src, dst):
    with open(src, "rb") as f_src:
        with open(dst, "wb") as f_dst:
            f_dst.write(f_src.read())


def apply_deploy_overlay(deploy_overlay_folder, deploy_folder):
    if not os.path.exists(deploy_overlay_folder):
        return

    for root, dirs, files in os.walk(deploy_overlay_folder):
        for file in files:
            src = os.path.join(root, file)
            rel = os.path.relpath(src, deploy_overlay_folder)

            if file.endswith(".append"):
                target_rel = rel[:-len(".append")]
                dst = os.path.join(deploy_folder, target_rel)
                os.makedirs(os.path.dirname(dst), exist_ok=True)
                with open(src, "r") as f_overlay:
                    overlay_content = f_overlay.read()
                if os.path.exists(dst):
                    with open(dst, "r") as f_target:
                        existing = f_target.read()
                    if not existing.endswith("\n") and existing:
                        overlay_content = "\n" + overlay_content
                with open(dst, "a") as f_target:
                    f_target.write(overlay_content)
                print(f"[overlay] APPEND: {target_rel}")

            elif file.endswith(".patch"):
                target_rel = rel[:-len(".patch")]
                dst = os.path.join(deploy_folder, target_rel)
                pset = patch_lib.fromfile(src)
                if pset and pset.apply(root=deploy_folder):
                    print(f"[overlay] PATCH applied: {target_rel}")
                else:
                    print(f"[overlay] PATCH FAILED: {target_rel}")

            else:
                dst = os.path.join(deploy_folder, rel)
                is_new = not os.path.exists(dst)
                os.makedirs(os.path.dirname(dst), exist_ok=True)
                shutil.copy2(src, dst)
                print(f"[overlay] {'NEW' if is_new else 'OVERWRITE'}: {rel}")


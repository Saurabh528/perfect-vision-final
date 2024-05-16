# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['card_callib_final.py'],
    pathex=[],
    binaries=[],
    datas=[('/Library/Frameworks/Python.framework/Versions/3.11/lib/python3.11/site-packages/mediapipe/modules', 'mediapipe/modules')],
    hiddenimports=[],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    [],
    exclude_binaries=True,
    name='card_callib_final',
    debug=False,
    strip=False,
    upx=False,
    console=True,  # Set to False if GUI application
)

coll = COLLECT(
    exe,
    a.binaries,
    a.zipfiles,
    a.datas,
    strip=False,
    upx=False,
    upx_exclude=[],
    name='card_callib_final',
)
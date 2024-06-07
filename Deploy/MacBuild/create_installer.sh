# mkdir -p payload/Applications
# mkdir -p payload/AdditionalDirectory
# echo "Copying application files to temporary directory..."
# cp -R ./PerfectVision.app payload/
# echo "Copying python files to temporary directory..."
# cp -R ./Python payload/AdditionalDirectory/
pkgbuild --root payload --identifier com.jatin.perfectvision --version 1.0 --install-location / PerfectVision.pkg

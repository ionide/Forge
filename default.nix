{ nixpkgs ? import <nixpkgs> {} }:
let
  inherit (nixpkgs) pkgs;
in
  {
  forge = pkgs.stdenv.mkDerivation {
    name = "Forge";
    src = pkgs.fetchFromGitHub {
      owner = "juselius";
      repo = "Forge";
      rev = "ab2155e";
      sha256 = "1sy7vq5v135xizz2mmsfdsm8aiij2rwkl0ab0k6zzi3m5aw8kfmq";
    };
    buildInputs = [
      pkgs.mono
      pkgs.fsharp
    ];
    configurePhase = ":";
    buildPhase = ''
      sed -i '
    /^run \$PAKET_EXE restore/i \
    export NuGetCachePath=`mktemp -d` \
    export NUGET_PACKAGES=$NuGetCachePath/packages
    /^run \$FAKE_EXE/a \
    rm -rf $NuGetCachePath
    ' build.sh
      sed -i '
        /==> ".*Tests"/d;
        /^"BuildTests"$/,/==> "Release"/d
      ' build.fsx
      ./build.sh
    '';
    installPhase = ''
      mkdir -p $out/bin
      mkdir -p $out/libexec
      cp -r temp $out/libexec/Forge
      ln -s $out/libexec/Forge/forge.sh $out/bin/forge
    '';
  };
}

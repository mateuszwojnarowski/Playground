# How to run
~~Prior to first run, go to IdentityServer and run `dotnet run /seed` to ensure db is seeded~~
Currently, docker container will always run with /seed option, to ensure that db is there
TODO: Change it so that it will seed db only if needed

# How to get certificate in the root of this project
This is to ensure that all moving parts are happy with some certificate. **THIS IS NOT PRODUCTION READY**

Run the following in Powershell:
> dotnet dev-certs https -ep "$pwd\aspnetcore-dev-cert.pfx" -p MyPw123
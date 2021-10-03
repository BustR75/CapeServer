# General
This Replaces the cape used by optifine with one specified by you

The hosts file on windows is `C:\Windows\System32\drivers\etc\hosts`

The hosts file on linux is `/etc/hosts`

The backup server for capes is Mantle

# Cape Server
Create your own local Cape Server others can connect to

Port forward port 80

Allow port 80 through firewall

Add `127.0.0.1 s.optifine.net` to hosts file on own computer

Add `IPADDRESS s.optifine.net` to hosts file on other computers

Run CapeServer.exe as Admin

## Who can see my cape
Anyone connected to the same cape server can see your cape

## I Want to upload my cape to a server
After adding the host to the hosts file

### Through the website
Go to s.optifine.net in a browser

In this you can look at capes that have been uploaded and upload your own cape

### Cape Uploader
Start `CapeUploader.exe "Path/to/Cape.png" USERNAME Address`

# Cape Git Repo
Uses a git repo as The cape server

Add `127.0.0.1 s.optifine.net` to hosts file on own computer

Run `CapeGitRepo.exe` as Admin for this repo as cape server

or Run `CapeGitRepo.exe USER/REPO` to use another repo as cape server

## Who can see my cape
Anyone running with the same repo can see your cape

## What do i need for a cape repo
all that is needed for the cape repo to work is a folder called capes with capes in it that isnt private

# I Want my Cape to be in Your Repo
Fork this repo

Add the cape to capes/USERNAME.PNG

Create Pull Request
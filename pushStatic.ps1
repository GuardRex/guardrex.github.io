param([string]$buildFolder, [string]$email, [string]$username, [string]$personalAccessToken)

Write-Host "- Set config settings...."
git config --global user.email $email
git config --global user.name $username
git config --global push.default matching

Write-Host "- Clone master branch...."
cd "$($buildFolder)\..\"
mkdir gh-deploy
git clone --quiet --branch=master https://$($username):$($personalAccessToken)@github.com/guardrex/guardrex.com.git .\gh-deploy\
cd gh-deploy\docs

Write-Host
Write-Host "Status:"
git status

Write-Host
Write-Host "- Clean docs folder...."
Get-ChildItem -Attributes !r | Remove-Item -Recurse -Force

Write-Host
Write-Host "Contents of gh-deploy\docs after clean:"
dir

Write-Host "- Copy contents of guardrex-com into docs folder...."
copy-item -path c:\projects\guardrex-com\docs\* -Destination c:\projects\gh-deploy\docs -Recurse

Write-Host
Write-Host "Contents of gh-deploy\docs after copy:"
dir

Write-Host
Write-Host "Status:"
git status

$thereAreChanges = git status | select-string -pattern "Changes not staged for commit:","Untracked files:" -simplematch
if ($thereAreChanges -ne $null) { 
    Write-host "- Committing changes..."
    #git add --all
    #git status
    #git commit -m "skip ci - static site regeneration"
    #git status
    #git push --quiet
    Write-Host "- Done!"
} 
else { 
    write-host "- No changes to commit"
}
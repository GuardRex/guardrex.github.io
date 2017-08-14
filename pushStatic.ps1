param([string]$buildFolder, [string]$email, [string]$username, [string]$personalAccessToken)

Write-Host "- Set config settings...."
git config --global user.email $email
git config --global user.name $username
git config --global push.default matching

Write-Host "- Clone master branch...."
cd "$($buildFolder)\..\"
mkdir gh-deploy
git clone --quiet --branch=master https://$($username):$($personalAccessToken)@github.com/guardrex/guardrex.com.git .\gh-deploy\
cd gh-deploy
git status

#Write-Host "- Clean docs folder...."
#Get-ChildItem -Attributes !r | Remove-Item -Recurse -Force

Write-Host "- Copy contents of docs_built folder into docs folder...."
copy-item -path ..\docs\* -Destination $pwd.Path\docs -Recurse

git status
$thereAreChanges = git status | select-string -pattern "Changes not staged for commit:","Untracked files:" -simplematch
if ($thereAreChanges -ne $null) { 
    Write-host "- Committing changes to documentation..."
    #git add --all
    #git status
    #git commit -m "skip ci - static site regeneration"
    #git status
    Write-Host "- Push it...."
    #git push --quiet
    Write-Host "- Pushed it good!"
} 
else { 
    write-host "- No changes to documentation to commit"
}
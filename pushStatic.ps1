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

Write-Host "1 contents gh-deploy\docs..."
dir

Write-Host "2 contents gh-deploy\docs\post..."
dir post

Write-Host "- Clean docs folder..."
Get-ChildItem -Attributes !r | Remove-Item -Recurse -Force

Write-Host "3 directory of c:\projects\guardrex-com\docs_staging..."
dir c:\projects\guardrex-com\docs_staging

Write-Host "4 directory of c:\projects\guardrex-com\docs_staging\post..."
dir c:\projects\guardrex-com\docs_staging\post

Write-Host "- Copy contents of guardrex-com into docs folder...."
copy-item -path c:\projects\guardrex-com\docs_staging\* -Destination c:\projects\gh-deploy\docs -Recurse -Force

Write-Host "5 directory of gh-deploy\docs after copy..."
dir

Write-Host "6 directory of gh-deploy\docs\post after copy..."
dir post

Write-Host "- Status:"
git status

$thereAreChanges = git status | select-string -pattern "Changes not staged for commit:","Untracked files:" -simplematch
if ($thereAreChanges -ne $null) { 
    Write-host "- Committing changes..."
    git add --all
    Write-Host "- Status:"
    git status
    git commit -m "[ci skip] GitHub Pages regeneration"
    Write-Host "- Status:"
    git status
    Write-host "- Pushing commit..."
    git push --quiet
    Write-Host "- Done!"
} 
else { 
    write-host "- No changes to commit"
}
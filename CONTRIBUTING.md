### Feature Development
It is assumed that all new features have been tested and deemed working **prior** to making a merge request.
Each feature, change, or major bugfix must have an accompanying issue. 

**Issue Details**
 - Label all issues (bug, feature, discussion)
 - Contents must contain the following
     + Description of the feature (what does it do? How?)
     + Why does the feature belong here? Can the same function be provided by a separate mod?
     + Do you view this as being a backwards incompatible change? If this release was reverted by the user after saving their game, will it cause problems?
     + Will this feature require a new mod dependency either at run time or compile time?
     + Outline iteration requirements
         * List each _releasable_ iteration as well as tasks for every major change required to implement this. It is assumed that developers will make reasonable efforts to release larger changes in sensible smaller releases, rather than dump  many high risk changes at once.
This issue will be used to track development progress on the feature, as well as brainstorming, discussion, and problem resolution as needed. Issues may be rejected if they are not sufficient. 

**Merge Requests**
Merge requests must be created when the change is ready for review. If the merge request is incomplete, or exhibits excessive flaws based on the below criteria, it will be closed until the developer has finished the change.


### Branching Overview
Master is for fully released changes. All versioned merges to master will be tagged vx.y.z. The dev branch will be used for incorporated features that have not yet been released.

### Code Standards
Following code standards from the start will increase the likelihood of a quick review process. For the most part, just follow common coding practices and pay particular attention to the following.
 - Use _spaces_ instead of tabs.
 - Attempt to follow the Single Responsibility principle. For the most part, methods should only do one thing, with intermediate steps being in separate methods or classes.
 - Avoid side-effects. Do not modify referenced parameters, always return a new instance instead of modifying parameters. 
 - Document every class and method. 
     + Document all return objects and parameters. 
     + Document any side effects if they cannot be avoided.
     + Document exceptional returns. Will this method return null if conditions are not met?
 - Use inline comments to describe sections of larger code blocks, but prefer self documenting code - do not comment what is obvious from reading the code itself.
 - Expose helper classes as public to allow other extensions to use them.
 - Follow existing conventions, even the dumb ones that aren't standard for c#. 
     + All conditionals must include curly braces
     + Opening curly brace must be in-line
 - Do not leave junk code, commented code, dead code, etc. Not even for historical purposes.
 - If there are TODO's, your change is incomplete
 - All transpiler patches must document the c# code before and after patching to make it clear at a glance what the patch is attempting to accomplish

### Commits and Branches
Review the following for excellent guidelines on writing good commit messages. (https://chris.beams.io/posts/git-commit/)  

**General Commit Guidelines**
 - Commit messages should document the change. Commit frequent small increments.
 - For larger commits (it happens) use the commit message body to detail what has changed and why
 - Do not squash commits

**Branch Guidelines**
 - Branches must be named with the following conventions
     + New features or updates to features: feature/branchname
     + Bugfixes: bugfix/branchname

**Merge Requests**
 - All merges must be reviewed and merged by a maintainer
 - All discussions must be closed/addressed prior to merge
 - Merges must not squash commits
 - Leave comments once discussions have been resolved
 - Fast forward merge the base branch into your branch before requesting review
 - One merge request **per feature**

# A profile with five completers, all staged: only styles and treadles apply in the draft, and each Sley applies after the first
# prompt, in slot order. Measure-Startup.ps1 dot-sources it after Import-Module PSLoom and times it; compare with typical.ps1.
Invoke-Loom -Draft {
  Thread Reed

  Style 'prompt:*' 'color' 'Cyan'
  Style 'prompt:*' 'symbol' '❯'
  Style 'completion:*' 'menu' 'select'
  Style 'completion:git' 'sort' $false

  Treadle glog { git log --oneline --graph --decorate }
  Treadle gst { git status --short }

  # The first completer used after the prompt: applied on the first idle slice.
  Shed -Wait
  Sley git -Alias g -Description 'Distributed version control' {
    OptionGroup Common {
      Option --verbose -Alias '-v'
      Option --quiet -Alias '-q'
    }

    Option --version
    Option --help -Alias '-h'

    Command commit -Alias ci -Description 'Record changes' {
      Use OptionGroup Common
      Option --message -Alias '-m' { Argument text }
      Option --amend
      Argument files -Variadic
    }

    Command checkout -Alias co -Description 'Switch branches' {
      Use OptionGroup Common
      Option --branch -Alias '-b' { Argument name }
      Argument target
    }

    Command log -Description 'Show commit logs' {
      Option --oneline
      Option --graph
      Option --max-count -Alias '-n' { Argument count }
    }
  }

  # Only on machines that have the tool.
  Shed -Slot 0b -RequiresCommand docker
  Sley docker -Description 'Containers' {
    Command run -Description 'Run a container' {
      Option --detach -Alias '-d'
      Option --name { Argument name }
      Option --publish -Alias '-p' { Argument ports }
      Argument image
    }

    Command ps -Description 'List containers' { Option --all -Alias '-a' }
    Command logs -Description 'Fetch container logs' { Option --follow -Alias '-f'; Argument container }
  }

  Shed -Slot 1a -RequiresCommand kubectl
  Sley kubectl -Alias k -Description 'Kubernetes' {
    Option --namespace -Alias '-n' { Argument namespace }
    Option --context { Argument context }

    Command get -Description 'Display resources' {
      Option --output -Alias '-o' { Argument format }
      Argument resource
    }

    Command apply -Description 'Apply a configuration' { Option --filename -Alias '-f' { Argument file } }
    Command logs -Description 'Print pod logs' { Option --follow -Alias '-f'; Argument pod }
  }

  Shed -Slot 1b -RequiresCommand dotnet
  Sley dotnet -Description '.NET CLI' {
    Command build -Description 'Build a project' {
      Option --configuration -Alias '-c' { Argument configuration }
      Argument project
    }

    Command test -Description 'Run tests' { Option --filter { Argument expression } }
    Command run -Description 'Run a project' { Option --project { Argument path } }
  }

  # Only inside a Node project, and quietly: a missing package.json is not worth a warning.
  Shed -Slot 2a -LoadIf { Test-Path package.json } -Silent
  Sley npm -Description 'Node package manager' {
    Command install -Alias i -Description 'Install packages' {
      Option --save-dev -Alias '-D'
      Argument packages -Variadic
    }

    Command run -Description 'Run a script' { Argument script }
    Command test -Alias t -Description 'Run tests'
  }
}

COMPILE_TARGET = "debug"
require "./utils.rb"

PRODUCT = "concurrent-lib"
CLR_VERSION = 'v4.0.30319'
MSBUILD_DIR = File.join(ENV['windir'].dup, 'Microsoft.NET', 'Framework', CLR_VERSION)

NUNIT_DIR = '../lib/nunit/bin/'

@nunitRunner = NUnitRunner.new :compile => COMPILE_TARGET, :nunit_dir => NUNIT_DIR

task :default => [:compile, :unit_test]

task :compile  do
  include FileTest
  
  buildRunner = MSBuildRunner.new(MSBUILD_DIR)
  buildRunner.compile :compilemode => COMPILE_TARGET, :solutionfile => '../concurrent-lib.sln'
Dir.mkdir 'output' unless exists?('output')

end

task :unit_test => [ :compile] do
   @nunitRunner.executeTests ['concurrent-lib.unit.tests']
end  





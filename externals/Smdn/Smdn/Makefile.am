
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build/bin/Debug/

SMDN_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.dll.mdb
SMDN_DLL_MDB=$(BUILD_DIR)/Smdn.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build/bin/Release/

SMDN_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	System.Runtime.CompilerServices/ExtensionAttribute.cs \
	Smdn.Mathematics/Fraction.cs \
	Smdn.Formats/Chars.cs \
	Smdn.Formats/Octets.cs \
	Smdn/Runtime.cs \
	Smdn.IO/PathUtils.cs \
	Smdn/ArrayExtensions.cs \
	Smdn/StringExtensions.cs \
	Mono.Simd/SimdRuntime.cs \
	Smdn/RuntimeEnvironment.cs \
	Smdn/EnumUtils.cs \
	Smdn/Action.cs \
	Smdn/Func.cs \
	Smdn.Mathematics/Radian.cs \
	Smdn.Mathematics/Polar.cs \
	Smdn/MimeType.cs \
	Smdn.IO/DirectoryInfoExtensions.cs \
	Smdn.IO/DirectoryUtils.cs \
	Smdn.Collections/IEnumerableExtensions.cs \
	Smdn.Threading/Parallel.cs \
	Smdn.Media/mmsystem.h.cs \
	Smdn/Platform.cs \
	Smdn/Endianness.cs \
	Smdn/Shell.cs \
	Smdn/Uuid.cs \
	Smdn/UuidVersion.cs \
	Smdn/Urn.cs \
	Smdn/Lazy.cs \
	Smdn.IO/StreamExtensions.cs \
	Smdn.Collections/ReadOnlyDictionary.cs \
	Smdn.Collections/IDictionaryExtensions.cs \
	Smdn/UnixTimeStamp.cs \
	Smdn/MathUtils.cs \
	Smdn.Formats/Csv.cs \
	Smdn.Formats/Hexadecimals.cs \
	Smdn/ConvertUtils.cs \
	Smdn/ByteString.cs \
	Smdn/ByteStringBuilder.cs \
	Smdn.IO/ChunkedMemoryStream.cs \
	Smdn.IO/PartialStream.cs \
	Smdn.IO/CachedStreamBase.cs \
	Smdn.IO/NonPersistentCachedStream.cs \
	Smdn.IO/PersistentCachedStream.cs \
	Smdn/Tuple.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Formats \
	Mono.Simd \
	Smdn.Collections \
	Smdn.Threading \
	smdn.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_PC = $(BUILD_DIR)/smdn.pc

$(eval $(call emit-deploy-wrapper,SMDN_PC,smdn.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)

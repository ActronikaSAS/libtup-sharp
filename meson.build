project('libtup-sharp', 'cs',
  version : '0.1',
  default_options : ['warning_level=3'])

src = [
  'src/context.cs',
  'src/message.cs',
  'src/native-message.cs',
  ]

lib = library('tup-sharp', src,
    install: true)

exe = executable('tup-sharp-test', 'samples/test.cs',
    link_with: lib)

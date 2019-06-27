// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const _ = require('lodash');
const mkdirp = require('mkdirp');
const path = require('path');

const { commonFilesWriter } = require('./commonFilesWriter');
const { BOT_TEMPLATE_NAME_CORE, BOT_TEMPLATE_NOPROMPT_CORE } = require('./constants');

// generators/app/templates folder name
const GENERATOR_TEMPLATE_NAME = 'core';

const LANG_JS = 'javascript';
const LANG_TS = 'typescript';

/**
 *
 * @param {String} language either 'javascript' or 'typescript'
 */
const getFolders = language => {
  if(!language || (_.toLower(language) !== LANG_JS && _.toLower(language) !== LANG_TS)) {
    throw new Error(`coreTemplateWriter.getFolders called for invalid language: ${ language }`);
  }

  let folders;
  if(_.toLower(language) === LANG_TS) {
    folders = [
      path.join('src', 'bots'),
      'cognitiveModels',
      path.join('src', 'dialogs'),
      'resources',
      'deploymentScripts',
    ];
  } else {
    folders = [
      'bots',
      'cognitiveModels',
      'dialogs',
      'resources',
    ];
  }
  return folders;
}

/**
 * Write the files that are specific to the core bot template
 *
 * @param {Generator} gen Yeoman's generator object
 * @param {String} templatePath file path to write the generated code
 */
const writeCoreTemplateFiles = (gen, templatePath) => {
  const BOTS_FOLDER = 0;
  const COGNITIVE_MODELS_FOLDER = 1;
  const DIALOGS_FOLDER = 2;
  const RESOURCES_FOLDER = 3;
  const DEPLOYMENT_SCRIPTS_FOLDER = 4;
  const TS_SRC_FOLDER = 'src';

  // get the folder strucure, based on language
  let srcFolders = [
    'bots',
    'cognitiveModels',
    'dialogs',
    'resources',
  ];
  // if we're generating TypeScript, then we need a deploymentScripts folder
  if(_.toLower(gen.props.language) === LANG_TS) {
    srcFolders = srcFolders.concat(['deploymentScripts']);
  }

  const destFolders = getFolders(_.toLower(gen.props.language));

  const extension = _.toLower(gen.props.language) === 'javascript' ? 'js' : 'ts';
  const srcFolder = _.toLower(gen.props.language) === 'javascript' ? '' : TS_SRC_FOLDER;
  // if we're generating JS, then keep the json extension
  // if we're generating TS, then we need the extension to be js or tsc will complain (tsc v3.1.6)
  const cardExtension = _.toLower(gen.props.language) === 'javascript' ? 'json' : 'js';

  // create the core bot folder structure
  for (let cnt = 0; cnt < destFolders.length; ++cnt) {
    mkdirp.sync(destFolders[cnt]);
  }

  // write out the bots folder
  let sourcePath = path.join(templatePath, srcFolders[BOTS_FOLDER]);
  let destinationPath = path.join(gen.destinationPath(), destFolders[BOTS_FOLDER]);
  gen.fs.copy(
    path.join(sourcePath, `dialogAndWelcomeBot.${extension}`),
    path.join(destinationPath, `dialogAndWelcomeBot.${extension}`),
    {
      botname: gen.props.botname
    }
  );
  gen.fs.copy(
    path.join(sourcePath, `dialogBot.${extension}`),
    path.join(destinationPath, `dialogBot.${extension}`)
  );

  // write out the LUIS model
  sourcePath = path.join(templatePath, srcFolders[COGNITIVE_MODELS_FOLDER]);
  destinationPath = path.join(gen.destinationPath(), destFolders[COGNITIVE_MODELS_FOLDER]);
  gen.fs.copy(
    path.join(sourcePath, 'FlightBooking.json'),
    path.join(destinationPath, 'FlightBooking.json')
  );

  // if we're writing out TypeScript, then we need to add a webConfigPrep.js
  if(_.toLower(gen.props.language) === LANG_TS) {
    sourcePath = path.join(templatePath, srcFolders[DEPLOYMENT_SCRIPTS_FOLDER]);
    destinationPath = path.join(gen.destinationPath(), destFolders[DEPLOYMENT_SCRIPTS_FOLDER]);
    gen.fs.copy(
      path.join(sourcePath, 'webConfigPrep.js'),
      path.join(destinationPath, 'webConfigPrep.js')
    );
  }

  // write out the dialogs folder
  sourcePath = path.join(templatePath, srcFolders[DIALOGS_FOLDER]);
  destinationPath = path.join(gen.destinationPath(), destFolders[DIALOGS_FOLDER]);
  if(_.toLower(gen.props.language) === LANG_TS) {
      gen.fs.copy(
      path.join(sourcePath, `bookingDetails.${extension}`),
      path.join(destinationPath, `bookingDetails.${extension}`)
    );
  }
  gen.fs.copy(
    path.join(sourcePath, `bookingDialog.${extension}`),
    path.join(destinationPath, `bookingDialog.${extension}`)
  );
  gen.fs.copy(
    path.join(sourcePath, `cancelAndHelpDialog.${extension}`),
    path.join(destinationPath, `cancelAndHelpDialog.${extension}`)
  );
  gen.fs.copy(
    path.join(sourcePath, `dateResolverDialog.${extension}`),
    path.join(destinationPath, `dateResolverDialog.${extension}`)
  );
  gen.fs.copy(
    path.join(sourcePath, `luisHelper.${extension}`),
    path.join(destinationPath, `luisHelper.${extension}`)
  );
  gen.fs.copy(
    path.join(sourcePath, `mainDialog.${extension}`),
    path.join(destinationPath, `mainDialog.${extension}`)
  );

  // write out the resources folder
  // which contains the welcome adaptive card
  sourcePath = path.join(templatePath, srcFolders[RESOURCES_FOLDER]);
  destinationPath = path.join(gen.destinationPath(), destFolders[RESOURCES_FOLDER]);
  gen.fs.copy(
    path.join(sourcePath, 'welcomeCard.json'),
    path.join(destinationPath, 'welcomeCard.json')
  );

  // write out the index.js and bot.js
  destinationPath = path.join(gen.destinationPath(), srcFolder);

  // gen index and main dialog files
  gen.fs.copyTpl(
    gen.templatePath(path.join(templatePath, `index.${extension}`)),
    path.join(destinationPath, `index.${extension}`),
    {
      botname: gen.props.botname
    }
  );
  // gen the logger stub
  if(_.toLower(gen.props.language) === LANG_TS) {
      gen.fs.copy(
      gen.templatePath(path.join(templatePath, `logger.${extension}`)),
      path.join(destinationPath, `logger.${extension}`)
    );
  }

}

/**
 * Write project files for Core template
 *
 * @param {Generator} gen Yeoman's generator object
 */
module.exports.coreTemplateWriter = gen => {
  // do some simple sanity checking to ensure we're being
  // called correctly
  const template = _.toLower(gen.props.template)
  if (template !== _.toLower(BOT_TEMPLATE_NAME_CORE) && template !== _.toLower(BOT_TEMPLATE_NOPROMPT_CORE)) {
    throw new Error(`coreTemplateWriter called for wrong template: ${ gen.props.template }`);
  }
  const templatePath = path.join(gen.templatePath(), GENERATOR_TEMPLATE_NAME);

  // write files common to all template options
  commonFilesWriter(gen, templatePath);

  // write files specific to the core bot template
  writeCoreTemplateFiles(gen, templatePath);
}

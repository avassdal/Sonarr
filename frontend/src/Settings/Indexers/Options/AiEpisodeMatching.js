import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

const providerOptions = [
  { key: 'openai', value: 'OpenAI' },
  { key: 'anthropic', value: 'Anthropic' },
  { key: 'gemini', value: 'Google Gemini' }
];

const modelOptions = {
  openai: [
    { key: 'gpt-4o-mini', value: 'GPT-4o Mini (Recommended)' },
    { key: 'gpt-4o', value: 'GPT-4o' },
    { key: 'gpt-4-turbo', value: 'GPT-4 Turbo' },
    { key: 'gpt-3.5-turbo', value: 'GPT-3.5 Turbo' }
  ],
  anthropic: [
    { key: 'claude-3-5-sonnet-20241022', value: 'Claude 3.5 Sonnet' },
    { key: 'claude-3-opus-20240229', value: 'Claude 3 Opus' },
    { key: 'claude-3-haiku-20240307', value: 'Claude 3 Haiku' }
  ],
  gemini: [
    { key: 'gemini-1.5-flash', value: 'Gemini 1.5 Flash (Most Cost-Effective)' },
    { key: 'gemini-1.5-pro', value: 'Gemini 1.5 Pro' },
    { key: 'gemini-1.0-pro', value: 'Gemini 1.0 Pro' }
  ]
};

function AiEpisodeMatching(props) {
  const {
    aiEpisodeMatchingEnabled,
    aiEpisodeMatchingProvider,
    aiEpisodeMatchingApiKey,
    aiEpisodeMatchingModel,
    onInputChange
  } = props;

  const isEnabled = aiEpisodeMatchingEnabled.value;
  const currentProvider = aiEpisodeMatchingProvider.value || 'openai';
  const currentModelOptions = modelOptions[currentProvider] || modelOptions.openai;

  return (
    <FieldSet legend={translate('AiEpisodeMatching')}>
      <Form>
        <FormGroup>
          <FormLabel>{translate('EnableAiEpisodeMatching')}</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="aiEpisodeMatchingEnabled"
            helpText={translate('EnableAiEpisodeMatchingHelpText')}
            helpLink="https://wiki.servarr.com/sonarr/settings#ai-episode-matching"
            onChange={onInputChange}
            {...aiEpisodeMatchingEnabled}
          />
        </FormGroup>

        {
          isEnabled &&
            <>
              <FormGroup>
                <FormLabel>{translate('AiProvider')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="aiEpisodeMatchingProvider"
                  values={providerOptions}
                  helpText={translate('AiProviderHelpText')}
                  onChange={onInputChange}
                  {...aiEpisodeMatchingProvider}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('AiApiKey')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.PASSWORD}
                  name="aiEpisodeMatchingApiKey"
                  helpText={translate('AiApiKeyHelpText')}
                  onChange={onInputChange}
                  {...aiEpisodeMatchingApiKey}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('AiModel')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="aiEpisodeMatchingModel"
                  values={currentModelOptions}
                  helpText={translate('AiModelHelpText')}
                  onChange={onInputChange}
                  {...aiEpisodeMatchingModel}
                />
              </FormGroup>
            </>
        }
      </Form>
    </FieldSet>
  );
}

AiEpisodeMatching.propTypes = {
  aiEpisodeMatchingEnabled: PropTypes.object.isRequired,
  aiEpisodeMatchingProvider: PropTypes.object.isRequired,
  aiEpisodeMatchingApiKey: PropTypes.object.isRequired,
  aiEpisodeMatchingModel: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default AiEpisodeMatching;
